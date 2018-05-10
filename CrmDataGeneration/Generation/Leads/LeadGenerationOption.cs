﻿using Bogus;
using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Leads
{
    public class LeadGenerationOption : GenerationOption<Lead>
    {
        public ProbabilityCollection<string> ConvertToOpportunityByStatus { get; set; }

        public override async System.Threading.Tasks.Task RunGeneration(GeneratorClient client, CancellationToken cancellationToken = default)
        {
            CheckSettings();
            var leads = await client.GenerateAll(this, cancellationToken);


        }

        protected async System.Threading.Tasks.Task ConvertLeadsToOpportunities(GeneratorClient client, IEnumerable<Lead> leads, CancellationToken cancellationToken = default)
        {
            if (ConvertToOpportunityByStatus.AsEnumerable.IsNullOrEmpty())
                return;

            var rand = new Randomizer(RandomizerSettings.Seed ?? client.Config.GlobalSeed);
            var wrappedClient = client.GetApiWrappedClient<Lead>();
            var actionClient = client.GetRawApiClient<InvokeActionClient>();
            foreach (var lead in leads)
            {
                if (!ConvertToOpportunityByStatus.Contains(lead.Status))
                    continue;

                var invocation = new ConvertLeadToOpportunity { Entity = lead };
                //rand.Bool()
                await wrappedClient.WrapAction(actionClient.ConvertLeadToOpportunityAsync(invocation, cancellationToken));
            }
        }
    }
}
