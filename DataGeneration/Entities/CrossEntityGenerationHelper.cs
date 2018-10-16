using DataGeneration.Common;
using DataGeneration.Entities.Activities;
using DataGeneration.Soap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Entities
{
    // !! no null checking in methods !!
    internal class CrossEntityGenerationHelper
    {
        protected static NLog.ILogger Logger => LogManager.GetLogger(LogManager.LoggerNames.GenerationRunner);

        internal static async Task<IProducerConsumerCollection<(string noteId, Entity entity)>> GetLinkEntitiesCollectionForActivityGeneration(
            IActivityGenerationSettings generationSettings,
            IApiClient apiClient,
            CancellationToken cancellationToken,
            Action<Entity> searchEntityAdjustment = null,
            bool returnEntities = false)
        {
            var entity = EntityHelper.InitializeFromType(generationSettings.EntityTypeForLinkedEntity);
            entity.ReturnBehavior = ReturnBehavior.OnlySpecified;
            searchEntityAdjustment?.Invoke(entity);
            EntityHelper.SetPropertyValue(entity, "NoteID", new GuidReturn());
            if (generationSettings.CreatedAtSearchRange != null)
            {
                var (start, end) = generationSettings.CreatedAtSearchRange.Value;
                if (start != null || end != null)
                {
                    var date = new DateTimeSearch();
                    if (start != null && end != null)
                    {
                        date.Value = start;
                        date.Value2 = end;
                        date.Condition = DateTimeCondition.IsBetween;
                    }
                    else if (start != null)
                    {
                        date.Value = start;
                        date.Condition = DateTimeCondition.IsGreaterThanOrEqualsTo;
                    }
                    else if (end != null)
                    {
                        date.Value = end;
                        date.Condition = DateTimeCondition.IsLessThanOrEqualsTo;
                    }

                    EntityHelper.SetPropertyValue(entity, "CreatedAt", date);
                }
            }

            var entities = await apiClient.GetListAsync(entity, cancellationToken);
            var search = (returnEntities
                ? entities.Select(e => (e.GetNoteId().ToString(), e))
                : entities.Select(e => (e.GetNoteId().ToString(), (Entity)null))
                ).ToArray();

            // adjust count
            if (generationSettings.EntitiesCountProbability != null)
            {
                var randomizer = RandomizerSettingsBase
                    .GetRandomizer(generationSettings.Seed ?? throw new InvalidOperationException()); //it should not fail

                var count = (int)(search.Length * generationSettings.EntitiesCountProbability);

                search = randomizer.ArrayElements(search, count).ToArray();

                generationSettings.Count = search.Length;

                Logger.Info("Count changed to {count}", generationSettings.Count);
            }

            return new ConcurrentQueue<(string, Entity)>(search);
        }

        internal static async Task<IDictionary<TKey, (string businessAccountId, int[] contactIds)[]>> GetBusinessAccountsWithLinkedContactsFromType<TKey>(
            IApiClient apiClient,
            Func<string, (TKey key, FetchOption fetch)> typeKeySelector, // select by type
            CancellationToken cancellationToken)
        {
            var accounts = await apiClient.GetListAsync(
                 new BusinessAccount
                 {
                     BusinessAccountID = new StringReturn(),
                     Type = new StringReturn(),
                     ReturnBehavior = ReturnBehavior.OnlySpecified
                 },
                 cancellationToken
            );

            var result = new Dictionary<TKey, (string, int[])[]>();

            foreach (var accountGroup in accounts.GroupBy(a => typeKeySelector(a.Type.Value)))
            {
                switch (accountGroup.Key.fetch)
                {
                    case FetchOption.Include:
                        result[accountGroup.Key.key] = accountGroup.Select(a => (a.BusinessAccountID.Value, (int[])null)).ToArray();
                        continue;
                    case FetchOption.IncludeInner:
                        var contacts = await apiClient.GetListAsync(
                            new Contact
                            {
                                ContactID = new IntReturn(),
                                BusinessAccount = new StringReturn(),
                                Active = new BooleanSearch { Value = true },
                                ReturnBehavior = ReturnBehavior.OnlySpecified
                            },
                            cancellationToken
                        );

                        result[accountGroup.Key.key] = accountGroup
                            .Select(a => (a.BusinessAccountID.Value, contacts: contacts
                                            .Where(c => c.BusinessAccount == a.BusinessAccountID)
                                            .Select(c => c.ContactID.Value ?? default)
                                            .ToArray()))
                            .Where(a => a.contacts != null && a.contacts.Length > 0)
                            .ToArray();
                        continue;
                    case FetchOption.Exlude:
                    default:
                        continue;
                }
            }

            return result;
        }

        internal enum FetchOption
        {
            Exlude,
            Include,
            IncludeInner,
        }
    }
}
