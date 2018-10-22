﻿using DataGeneration.Common;
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
    public class LinkEmailsGenerationRunner : EntitiesSearchGenerationRunner<LinkEmails, LinkEmailsGenerationSettings>
    {
        public LinkEmailsGenerationRunner(ApiConnectionConfig apiConnectionConfig, LinkEmailsGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        protected override void UtilizeFoundEntities(IList<Entity> entities)
        {
            GenerationSettings.RandomizerSettings.LinkEntities = new ConcurrentQueue<Entity>(entities);
        }

        protected override async VoidTask GenerateSingle(IApiClient client, LinkEmails entity, CancellationToken cancellationToken)
        {
            var noteId = entity.LinkEntity.GetNoteId().ToString();
            if (noteId.IsNullOrEmpty())
                throw new InvalidOperationException("NoteId must be not empty for linked entity.");

            foreach (var email in entity.Emails)
            {
                email.ReturnBehavior = ReturnBehavior.OnlySystem;
                var resEmail = await client.PutAsync(email, cancellationToken);

                var link = new LinkEntityToEmail
                {
                    RelatedEntity = noteId,
                    Type = GenerationSettings.PxTypeForLinkedEntity
                };

                await client.InvokeAsync(resEmail, link, cancellationToken);
            }
        }
    }
}