using DataGeneration.Core.Api;
using DataGeneration.Core.Queueing;
using DataGeneration.Core.Settings;
using DataGeneration.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataGeneration.Core.DataGeneration;

namespace DataGeneration.Core
{
    // provides search in RunBeforeGeneration
    // and changes count
    public abstract class EntitiesSearchGenerationRunner<TEntity, TGenerationSettings> : GenerationRunner<TEntity, TGenerationSettings>
        where TEntity : class
        where TGenerationSettings : class, IGenerationSettings<TEntity>, ISearchUtilizer
    {
        protected EntitiesSearchGenerationRunner(ApiConnectionConfig apiConnectionConfig, TGenerationSettings generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        // override to true if no need to GetEntities in RunBeforeGeneration
        protected virtual bool SkipEntitiesSearch => false;
        // override if no need to execute complex query for entities marked with IComplexQueryEntity interface
        protected virtual bool IgnoreComplexQueryEntity => false;
        protected virtual bool IgnoreAdjustReturnBehavior => false;
        protected abstract void UtilizeFoundEntities(IList<Soap.Entity> entities);
        protected virtual void AdjustEntitySearcher(EntitySearcher searcher)
        {
            searcher.AdjustInput(adj =>
                        adj.Adjust(e => e.ReturnBehavior = Soap.ReturnBehavior.OnlySpecified)
                           .AdjustIf(!IgnoreAdjustReturnBehavior, adj_ =>
                                adj_.AdjustIfIs<Soap.IAdjustReturnBehaviorEntity>(e => e.AdjustReturnBehavior())));
        }

        protected override async Task RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            await base.RunBeforeGeneration(cancellationToken);

            if (SkipEntitiesSearch)
                return;

            var entities = await GetEntities(GenerationSettings.SearchPattern, AdjustEntitySearcher, cancellationToken);

            if (!IgnoreComplexQueryEntity)
            {
                var complexEntities = entities.OfType<Soap.IComplexQueryEntity>();
                if (complexEntities.Any())
                {
                    await GetComplexQueryExecutor().Execute(complexEntities, cancellationToken);
                }
            }

            UtilizeFoundEntities(entities);

            if (GenerationSettings.RandomizerSettings is IAvailableCountLimit limit
                && limit.AvailableCount.HasValue(out var avail)
                && avail < GenerationSettings.Count)
            {
                ChangeGenerationCount(avail, "Randomizer settings has count limit");
            }
        }

        protected override void ValidateGenerationSettings()
        {
            base.ValidateGenerationSettings();
            if (!SkipEntitiesSearch && GenerationSettings.SearchPattern is null)
                throw new ValidationException($"Property {nameof(SearchPattern)} of {nameof(GenerationSettings)} must be not null in order to search entities in {nameof(RunBeforeGeneration)}");
        }

        protected EntitySearcher GetEntitySearcher(Func<Soap.Entity> factory)
        {
            return new EntitySearcher(GetListFactory, factory);
        }

        protected EntitySearcher GetEntitySearcher(string entityType) => GetEntitySearcher(() => EntityHelper.InitializeFromType(entityType));


        protected ComplexQueryExecutor GetComplexQueryExecutor() => new ComplexQueryExecutor(GetListFactory);

        protected async Task<IEnumerable<Soap.Entity>> GetListFactory(Soap.Entity entity, CancellationToken ct)
        {
            using (var client = await GetLoginLogoutClient(ct))
            {
                return await client.GetListAsync(entity, ct);
            }
        }

        protected Task<IList<Soap.Entity>> GetEntities(SearchPattern searchPattern, CancellationToken ct)
        {
            return GetEntities(searchPattern, null, ct);
        }

        protected async Task<IList<Soap.Entity>> GetEntities(SearchPattern searchPattern, Action<EntitySearcher> searcherAdjustment, CancellationToken ct)
        {
            if (searchPattern == null)
                throw new ArgumentNullException(nameof(searchPattern));


            var searcher = GetEntitySearcher(searchPattern.EntityType);
            searchPattern.AdjustSearcher(searcher);
            searcherAdjustment?.Invoke(searcher);

            return await searcher.ExecuteSearch(ct);
        }

    }
}