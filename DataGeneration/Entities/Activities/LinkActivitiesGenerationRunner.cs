﻿using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Common;
using DataGeneration.Soap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VoidTask = System.Threading.Tasks.Task;

namespace DataGeneration.Entities.Activities
{
    public class LinkActivitiesGenerationRunner : EntitiesSearchGenerationRunner<OneToManyRelation<LinkEntityToActivity, Activity>, LinkActivitiesGenerationSettings>
    {
        public LinkActivitiesGenerationRunner(ApiConnectionConfig apiConnectionConfig, LinkActivitiesGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            ChangeGenerationCount(entities.Count, "To be equal to found entities count.");

            GenerationSettings.RandomizerSettings.LinkEntities = new ConcurrentQueue<Entity>(entities);
        }

        protected override async VoidTask GenerateSingle(IApiClient client, OneToManyRelation<LinkEntityToActivity, Activity> entity, CancellationToken ct)
        {
            foreach (var activity in entity.Right)
            {
                await client.InvokeAsync(await client.PutAsync(activity, ct), entity.Left, ct);
            }
        }

        protected override void LogResultsArgs(out string entity, out string parentEntity, out string action)
        {
            entity = "Activity";
            parentEntity = GenerationSettings.SearchPattern?.EntityType?.Split('.').Last();
            action = "Create and Link";
        }
    }
}
