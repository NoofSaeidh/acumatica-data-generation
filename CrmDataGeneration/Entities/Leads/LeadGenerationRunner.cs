using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.Soap;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VoidTask = System.Threading.Tasks.Task;

namespace CrmDataGeneration.Entities.Leads
{
    public class LeadGenerationRunner : GenerationRunner<Lead, LeadGenerationSettings>
    {
        public LeadGenerationRunner(ApiConnectionConfig apiConnectionConfig, LeadGenerationSettings generationSettings)
            : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override async VoidTask GenerateSingle(IApiClient client, Lead entity, CancellationToken cancellationToken)
        {
            var sw = new StopwatchLogger().Start();

            entity.ReturnBehavior = ReturnBehavior.OnlySpecified;
            entity.NoteID = new GuidReturn();
            var resultLead = await client.PutAsync(entity, cancellationToken);

            sw.Log("Put Lead").Restart();

            // convert entity
            var convertFlags = GetConvertFlags(entity);

            sw.Restart();

            if (convertFlags.HasFlag(ConvertLeadFlags.ToOpportunity))
                await client.InvokeAsync(resultLead, new ConvertLeadToOpportunity(), cancellationToken);

            sw.Log("Convert Lead");


            // create emails
            var emails = PrepareEmailsForCreation(entity);
            if (emails != null)
            {
                foreach (var email in emails)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    email.ReturnBehavior = ReturnBehavior.OnlySystem;
                    sw.Restart();
                    var createdEmail = await client.PutAsync(email, cancellationToken);
                    sw.Log("Put Email").Restart();
                    await client.InvokeAsync(
                        createdEmail,
                        new LinkEntityToEmail
                        {
                            RelatedEntity = resultLead.NoteID.ToString(),
                            Type = GenerationSettings.PxTypeName
                        }
                    );
                    sw.Log("Link Email");
                }
            }
        }

        private ConvertLeadFlags GetConvertFlags(Lead lead)
        {
            // convert depending on Probability defined in ConvertToOpportunityByStatus

            var byConversion = GenerationSettings
                .ConvertByStatuses
                ?.SelectMany(x => x.Value.AsDictionary,
                    (x, y) => new { conversion = y.Key, status = x.Key, probability = y.Value })
                .GroupBy(x => x.conversion);
            //.ToDictionary(x => x.conversion, x => new { x.status, x.probability });
            if (byConversion == null)
                return ConvertLeadFlags.DontConvert;

            var result = ConvertLeadFlags.DontConvert;

            foreach (var conversion in byConversion)
            {
                var conv = conversion.FirstOrDefault(c => c.status == lead.Status);
                if (conv == null)
                    continue;
                if (Randomizer.Bool((float)conv.probability))
                    result |= conv.conversion;
            }
            return result;
        }

        private IEnumerable<Email> PrepareEmailsForCreation(Lead lead)
        {
            var emailSettings = GenerationSettings.EmailsGenerationSettings;
            if (emailSettings == null)
                yield break;
            emailSettings.Validate();

            var emailsCount = Randomizer.ProbabilityRandomIfAny(emailSettings.EmailsForSingleLeadCounts);
            if (emailsCount == 0)
                yield break;

            var emails = emailSettings.EmailRandomizerSettings.GetStatefullDataGenerator().GenerateList(emailsCount);
            foreach (var email in emails)
            {
                email.Incoming = true;
                email.From = lead.Email;
                email.To = Randomizer.ProbabilityRandomIfAny(emailSettings.SystemAccounts).Email;
                yield return email;

                var outEmail = emailSettings.EmailRandomizerSettings.GetStatefullDataGenerator().Generate();
                outEmail.Incoming = false;
                outEmail.From = Randomizer.ProbabilityRandomIfAny(emailSettings.SystemAccounts).Email;
                outEmail.To = lead.Email;
                yield return outEmail;
            }
        }
    }
}
