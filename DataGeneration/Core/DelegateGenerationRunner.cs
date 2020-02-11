using Bogus;
using DataGeneration.Core.Api;
using DataGeneration.Core.Queueing;
using DataGeneration.Core.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Core
{
    public static class DelegateGenerationSettings
    {
        public static DelegateGenerationSettings<TEntity> Create<TEntity>(
            Func<Faker<TEntity>, Faker<TEntity>> randomizeDelegate,
            Func<IApiClient, TEntity, CancellationToken, Task> generateDelegate)
            where TEntity : class
        {
            if (randomizeDelegate is null)
                throw new ArgumentNullException(nameof(randomizeDelegate));
            if (generateDelegate is null)
                throw new ArgumentNullException(nameof(generateDelegate));

            return new DelegateGenerationSettings<TEntity>
            {
                RandomizerSettings = new DelegateRandomizerSettings<TEntity>
                {
                    GetFakerDelegate = randomizeDelegate,
                },
                GenerateSingleDelegate = generateDelegate,
            };
        }

        public static DelegateGenerationSettings<TEntity> Create<TEntity>(
            TEntity dummy,
            Func<Faker<TEntity>, Faker<TEntity>> randomizeDelegate,
            Func<IApiClient, TEntity, CancellationToken, Task> generateDelegate)
            where TEntity : class
        {
            if (randomizeDelegate is null)
                throw new ArgumentNullException(nameof(randomizeDelegate));
            if (generateDelegate is null)
                throw new ArgumentNullException(nameof(generateDelegate));

            return new DelegateGenerationSettings<TEntity>
            {
                RandomizerSettings = new DelegateRandomizerSettings<TEntity>
                {
                    GetFakerDelegate = randomizeDelegate,
                },
                GenerateSingleDelegate = generateDelegate,
            };
        }
    }

    public class DelegateRandomizerSettings<TEntity> : RandomizerSettings<TEntity> 
        where TEntity : class
    {
        [Required]
        public Func<Faker<TEntity>, Faker<TEntity>> GetFakerDelegate { get; set; }

        protected override Faker<TEntity> GetFaker()
        {
            return GetFakerDelegate(base.GetFaker());
        }
    }

    public class DelegateGenerationSettings<TEntity> : GenerationSettings<TEntity, DelegateRandomizerSettings<TEntity>>, ISearchUtilizer
         where TEntity : class
    {
        public override bool CanCopy => false;
        public override bool CanInject => false;

        [Required]
        public Func<IApiClient, TEntity, CancellationToken, Task> GenerateSingleDelegate { get; set; }

        public SearchPattern SearchPattern { get; set; }


        public override GenerationRunner GetGenerationRunner(ApiConnectionConfig apiConnectionConfig) => new DelegateGenerationRunner<TEntity>(apiConnectionConfig, this);
    }

    public class DelegateGenerationRunner<TEntity> : EntitiesSearchGenerationRunner<TEntity, DelegateGenerationSettings<TEntity>>
         where TEntity : class
    {
        public DelegateGenerationRunner(ApiConnectionConfig apiConnectionConfig, DelegateGenerationSettings<TEntity> generationSettings) : base(apiConnectionConfig, generationSettings)
        {
        }

        // probably implement later
        protected override bool SkipEntitiesSearch => true;
        protected override bool IgnoreComplexQueryEntity => true;
        protected override bool IgnoreAdjustReturnBehavior => true;

        protected override Task GenerateSingle(IApiClient client, TEntity entity, CancellationToken ct)
        {
            return GenerationSettings.GenerateSingleDelegate(client, entity, ct);
        }

        protected override void UtilizeFoundEntities(IList<Soap.Entity> entities)
        {
        }
    }
}
