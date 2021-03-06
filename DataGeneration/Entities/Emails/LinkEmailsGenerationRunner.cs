﻿using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Common;
using DataGeneration.Core.Helpers;
using DataGeneration.Core.Queueing;
using DataGeneration.Soap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Emails
{
    public class LinkEmailsGenerationRunner : EntitiesSearchGenerationRunner<OneToManyRelation<LinkEntityToEmail, OneToManyRelation<Email, File>>, LinkEmailsGenerationSettings>
    {
        public LinkEmailsGenerationRunner(ApiConnectionConfig apiConnectionConfig, LinkEmailsGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            // not in AdjustEntitySearcher because email may be taken (as in case) from other entities 
            // and it is processed only after search
            entities = entities
                .GetEnumerableAdjuster()
                .AdjustCast<IEmailEntity>(en => en.Where(e => e.Email != null))
                .Value
                .ToList();

            ChangeGenerationCount(entities.Count, "To be equal to found entities count.");

            GenerationSettings.RandomizerSettings.LinkEntities = new ConcurrentQueue<Entity>(entities);
        }

        protected override async VoidTask RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            await base.RunBeforeGeneration(cancellationToken);

            GenerationSettings.RandomizerSettings.EmbeddedFilesTags = await PutEmbeddedFiles(cancellationToken);
        }


        protected override async VoidTask GenerateSingle(IApiClient client, OneToManyRelation<LinkEntityToEmail, OneToManyRelation<Email, File>> entity, CancellationToken ct)
        {
            foreach (var relation in entity.Right)
            {
                if (relation.Right.IsNullOrEmpty())
                {
                    await client.InvokeAsync(
                        await client.PutAsync(relation.Left, ct), 
                        entity.Left, 
                        ct
                    );
                }
                else
                {
                    var email = await client.PutAsync(relation.Left, ct);
                    await client.PutFilesAsync(email, relation.Right, ct);
                    await client.InvokeAsync(email, entity.Left, ct);
                }
            }
        }

        #region Embedded images

        protected async Task<OneToManyRelation<Email, File>> PutEmbeddedFiles(CancellationToken ct)
        {
            // create base entity (email) for linked images for all other emails
            if (GenerationSettings.RandomizerSettings.AttachmentsLocation == null
                || !GenerationSettings.RandomizerSettings.BaseEntityEmbeddedImagesAttachedCount.HasValue(out var count)
                || count <= 0)
                return null;
            FileLoader loader;
            try
            {
                loader = new FileLoader(GenerationSettings.RandomizerSettings.AttachmentsLocation);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Cannot load files");
                return null;
            }
            var randomizer = GenerationSettings.RandomizerSettings.GetRandomizer();
            var files = randomizer.Shuffle(loader.GetAllFiles("*.jpg"))
                // exclude files with the same names
                .GroupBy(f => f.Name)
                .Take(count)
                .Select(f => new File { Name = f.First().Name, Content = loader.LoadFile(f.First()) })
                .ToArray();
            using (var client = await GetLoginLogoutClient(ct))
            {
                var email = await client.PutAsync(new Email
                {
                    Subject = "Base email for storing images " + Guid.NewGuid().ToString(),
                    From = "a@a.a",
                    To = "a@a.a",
                    ReturnBehavior = ReturnBehavior.All
                }, ct);

                try
                {
                    await client.PutFilesAsync(email, files, ct);
                }
                finally
                {
                    try
                    {
                        // first just sets status of email, second fully deletes
                        await client.DeleteAsync(email);
                        await client.DeleteAsync(email);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Cannot delete temp entity, {entity}", email);
                    }
                }

                return new OneToManyRelation<Email, File>(email, files);
            }
        }

        #endregion

        
        protected override void LogResultsArgs(out string entity, out string parentEntity, out string action)
        {
            entity = "Email";
            parentEntity = GenerationSettings.SearchPattern?.EntityType?.Split('.').Last();
            action = "Create and Link";
        }
    }
}
