using Bogus;
using DataGeneration.Core;
using DataGeneration.Core.Common;
using DataGeneration.Core.Helpers;
using DataGeneration.Core.Settings;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DataGeneration.Entities.Emails
{
    public class LinkEmailsRandomizerSettings : RandomizerSettings<OneToManyRelation<LinkEntityToEmail, OneToManyRelation<Email, File>>>
    {
        // todo: seems like manual date time doesn't work
        public (DateTime startDate, DateTime endDate)? DateRange { get; set; }

        [RequiredCollection(AllowEmpty = false)]
        public ProbabilityCollection<(int min, int max)> EmailCountPerEntity { get; set; }

        [Required]
        public string SystemEmailAddress { get; set; }

        [Required]
        public string PxTypeForLinkedEntity { get; set; }

        public string AttachmentsLocation { get; set; }

        public ProbabilityCollection<(int min, int max)> AttachmentsCount { get; set; }

        // also used as count of combination of paragraphs and embedded images
        public ProbabilityCollection<(int min, int max)> ParagraphsCount { get; set; }

        // for RunBeforeGeneration -> to inject into EmbeddedFiles
        public int? BaseEntityEmbeddedImagesAttachedCount { get; set; }

        // injected
        [JsonIgnore]
        public IProducerConsumerCollection<Entity> LinkEntities { get; set; }

        [JsonIgnore]
        public OneToManyRelation<Email, File> EmbeddedFilesTags { get; set; }

        protected override Faker<OneToManyRelation<LinkEntityToEmail, OneToManyRelation<Email, File>>> GetFaker()
        {
            // need to create incoming email for each outgoing email
            // so need to persist outgoing and check in each step
            Email incomingEmail = null;

            var emailFaker = GetFaker<Email>()
                .Rules((f, e) =>
                {
                    e.ReturnBehavior = ReturnBehavior.None;

                    var paragraphs = f.Random.Int(f.Random.ProbabilityRandomIfAny(ParagraphsCount));

                    if (EmbeddedFilesTags != null && paragraphs > 0)
                    {
                        // don't want to expose this
                        var heights = new int[] { 100, 150, 200 };
                        var widths = new int[] { 100, 150, 200 };
                        var imgSource = EmbeddedFilesTags.Right.Select(file =>
                            (f.Lorem.Paragraphs(), file.Name, file.Name.Replace(".jpg", ""),
                                f.PickRandom(heights), f.PickRandom(widths))).Take(paragraphs);
                        e.Body = GenerateHtmlBodyWithImgs(EmbeddedFilesTags.Left.NoteID.Value.ToString(), imgSource);
                    }
                    else if (paragraphs > 0)
                        e.Body = EncodeText(f.Lorem.Paragraphs(paragraphs));
                    else
                        e.Body = EncodeText(f.Lorem.Paragraphs());

                    e.MailStatus = "Processed";

                    // create incoming email
                    if (incomingEmail == null)
                    {
                        e.Incoming = true;
                        e.To = SystemEmailAddress;
                        e.Subject = f.Lorem.Sentence(3, 10);

                        if (DateRange != null)
                        {
                            var (start, end) = DateRange.Value;
                            e.Date = f.Date.Between(start, end);
                        }

                        incomingEmail = e;
                    }
                    // create outgoing email  (for previous incoming)
                    else
                    {
                        e.Incoming = false;
                        e.Subject = $"RE: {incomingEmail.Subject}";
                        e.From = SystemEmailAddress;
                        e.Date = incomingEmail.Date.Value.Value.AddDays(1);

                        incomingEmail = null;
                    }
                });

            var files = GetEndlessAttachementsFiles();
            File[] getFiles(Email email, Faker faker)
            {
                // but maybe should generate for every type
                if(email.Incoming == false)
                {
                    return new File[0];
                }

                var (min, max) = faker.Random.ProbabilityRandomIfAny(AttachmentsCount);
                var count = faker.Random.Int(min, max);
                return files(faker).Take(count).ToArray();
            }

            return base
                .GetFaker()
                .CustomInstantiator(f =>
                {
                    var (min, max) = f.Random.ProbabilityRandom(EmailCountPerEntity);
                    var count = f.Random.Int(min, max) * 2;
                    var emails = emailFaker.Generate(count);

                    if (!LinkEntities.TryTake(out var linkEntity))
                    {
                        throw new GenerationException("Cannot generate entities relation. No entities to link remain.");
                    }

                    Debug.Assert(linkEntity is IEmailEntity, "linkEntity is not IEmailEntity. Email will be null!");
                    if (linkEntity is IEmailEntity emailEntity)
                    {
                        Debug.Assert(emailEntity.Email != null, "Email should not be null!");

                        emails.ForEach(e =>
                        {
                            if ((bool)e.Incoming)
                            {
                                e.From = emailEntity.Email;
                            }
                            else
                            {
                                e.To = emailEntity.Email;
                            }
                        });
                    }
                    var noteId = linkEntity.GetNoteId().ToString();
                    if (noteId.IsNullOrEmpty())
                        throw new InvalidOperationException("NoteId must be not empty for linked entity.");

                    var link = new LinkEntityToEmail
                    {
                        Type = PxTypeForLinkedEntity,
                        RelatedEntity = noteId
                    };

                    var relations = emails.Select(e => new OneToManyRelation<Email, File>(e, getFiles(e, f))).ToArray();

                    return new OneToManyRelation<LinkEntityToEmail, OneToManyRelation<Email, File>>(link, relations);
                });
        }

        // return function to not to check initialization and create loader each iteration and it need faker
        private Func<Faker, IEnumerable<File>> GetEndlessAttachementsFiles()
        {
            if (AttachmentsLocation == null)
            {
                Logger.Info($"{nameof(AttachmentsLocation)} is null, no images will be generated");
                return _ => Enumerable.Empty<File>();
            }
            if (AttachmentsCount == null)
                return _ => Enumerable.Empty<File>();

            FileLoader fileLoader = new CachedFileLoader(AttachmentsLocation);
            var imageFiles = fileLoader.GetAllFiles();
            if (imageFiles.Length == 0)
                throw new InvalidOperationException($"Directory {AttachmentsLocation} doesn't contain files.");
            Logger.Trace("Files found for emails attachments: {count}", imageFiles.Length);

            IEnumerable<File> result(Faker faker)
            {
                while (true)
                {
                    var file = faker.PickRandom(imageFiles);
                    var imageContent = fileLoader.LoadFile(file);
                    var resFile = new File
                    {
                        Content = imageContent,
                        Name = file.Name
                    };
                    yield return resFile;
                }
            }
            return result;
        }

        protected string GetImgTag(string noteId, string imgName, string title, int width, int height)
        {
            return
                $"<img src=\"Email Activity ({noteId})\\{imgName}\" " +
                      "objtype=\"attached\" " +
                      "data-convert=\"view\" " +
                     $"title=\"{title}\" " +
                     $"width=\"{width}\" " +
                     $"height=\"{height}\">";
        }

        protected string GenerateHtmlBodyWithImgs(
            string noteId, 
            IEnumerable<(string beforeText, string imgName, string title, int width, int height)> textWithImages)
        {
            var builder = new StringBuilder();
            foreach (var (text, imgName, title, width, height) in textWithImages)
            {
                builder.Append(EncodeText(text));
                builder.Append(EncodeToPTag(GetImgTag(noteId, imgName, title, width, height)));
            }
            return builder.ToString();
        }

        protected string EncodeText(string text)
        {
            var splitedText = text.Replace("\r\n", "\n").Split('\n');
            return string.Join(" ", splitedText.Select(t => EncodeToPTag(t.IsNullOrWhiteSpace() ? "<br>" : t)));
        }

        private string EncodeToPTag(string text)
        {
            return $"<p class=\"richp\">{text}</p>";
        }
    }
}