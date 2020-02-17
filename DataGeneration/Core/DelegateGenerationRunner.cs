using Bogus;
using DataGeneration.Core.Api;
using DataGeneration.Core.Queueing;
using DataGeneration.Core.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration.Core
{
    public static class DelegateGenerationSettings
    {
        public delegate Faker<TEntity> FakerDelegate<TEntity>(DelegateRandomizerSettings<TEntity> @this, Faker<TEntity> faker) where TEntity : class;
        public delegate Task ApiClientDelegate<TEntity>(DelegateGenerationRunner<TEntity> @this, IApiClient apiClient, CancellationToken ct) where TEntity : class;
        public delegate Task ApiClientWithEntityDelegate<TEntity>(DelegateGenerationRunner<TEntity> @this, IApiClient apiClient, TEntity entity, CancellationToken ct) where TEntity : class;


        public static DelegateGenerationSettings<TEntity> Create<TEntity>(
            FakerDelegate<TEntity> randomizeDelegate,
            ApiClientWithEntityDelegate<TEntity> generateDelegate,
            ApiClientDelegate<TEntity> beforeGenerateDelegate = null,
            ApiClientDelegate<TEntity> afterGenerateDelegate = null)
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
                RunBeforeGenerationDelegate = beforeGenerateDelegate,
                RunAfterGenerationDelegate = afterGenerateDelegate,
            };
        }

        public static DelegateGenerationSettings<TEntity> Create<TEntity>(
            TEntity dummy,
            FakerDelegate<TEntity> randomizeDelegate,
            ApiClientWithEntityDelegate<TEntity> generateDelegate,
            ApiClientDelegate<TEntity> beforeGenerateDelegate = null,
            ApiClientDelegate<TEntity> afterGenerateDelegate = null)
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
                RunBeforeGenerationDelegate = beforeGenerateDelegate,
                RunAfterGenerationDelegate = afterGenerateDelegate,
            };
        }
    }

    public class DelegateRandomizerSettings<TEntity> : RandomizerSettings<TEntity> 
        where TEntity : class
    {
        [Required]
        [JsonIgnore]
        public DelegateGenerationSettings.FakerDelegate<TEntity> GetFakerDelegate { get; set; }

        protected override Faker<TEntity> GetFaker()
        {
            return GetFakerDelegate(this, base.GetFaker());
        }
    }

    public class DelegateGenerationSettings<TEntity> : GenerationSettings<TEntity, DelegateRandomizerSettings<TEntity>>, ISearchUtilizer
         where TEntity : class
    {
        public override bool CanCopy => false;
        public override bool CanInject => false;


        [Required]
        [JsonIgnore]
        public DelegateGenerationSettings.ApiClientWithEntityDelegate<TEntity> GenerateSingleDelegate { get; set; }
        [JsonIgnore]
        public DelegateGenerationSettings.ApiClientDelegate<TEntity> RunBeforeGenerationDelegate { get; set; }
        [JsonIgnore]
        public DelegateGenerationSettings.ApiClientDelegate<TEntity> RunAfterGenerationDelegate { get; set; }


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
            return GenerationSettings.GenerateSingleDelegate(this, client, entity, ct);
        }

        protected override async Task RunBeforeGeneration(CancellationToken cancellationToken = default)
        {
            var del = GenerationSettings.RunBeforeGenerationDelegate;
            if(del is null)
                return;

            using (var client = await GetLoginLogoutClient(cancellationToken))
            {
                await del(this, client, cancellationToken);
            }
        }

        protected override async Task RunAfterGeneration(CancellationToken cancellationToken = default)
        {
            var del = GenerationSettings.RunAfterGenerationDelegate;
            if (del is null)
                return;

            using (var client = await GetLoginLogoutClient(cancellationToken))
            {
                await del(this, client, cancellationToken);
            }
        }

        protected override void UtilizeFoundEntities(IList<Soap.Entity> entities)
        {
        }

        public new void ChangeGenerationCount(int count, string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            base.ChangeGenerationCount(count, message, memberName, sourceFilePath, sourceLineNumber);
        }
    }
}
