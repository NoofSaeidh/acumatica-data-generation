using CrmDataGeneration.OpenApi;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Common
{
    public abstract class ApiWrappedClient
    {
        protected ApiWrappedClient(OpenApiState openApiState)
        {
            OpenApiState = openApiState ?? throw new ArgumentNullException(nameof(openApiState));
        }
        protected OpenApiState OpenApiState { get; }

        protected static ILogger Logger => LogConfiguration.DefaultLogger;

    }

    public abstract class ApiWrappedClient<T> : ApiWrappedClient, IApiWrappedClient<T> where T : OpenApi.Reference.Entity
    {
        private const string _wrappedAction = "Wrapped action";

        protected ApiWrappedClient(OpenApiState openApiState) : base(openApiState)
        {
        }

        public async Task<T> CreateSingle(T entity, CancellationToken cancellationToken = default)
        {
            return await ProcessSingle("Create single", entity, async (e, t) => await CreateRaw(e, cancellationToken), cancellationToken);
        }

        public async Task<IEnumerable<T>> CreateAll(IEnumerable<T> entities, ExecutionTypeSettings executionTypeSettings, CancellationToken cancellationToken = default)
        {
            return await ProcessAll("Create all", entities, executionTypeSettings, (e, t) => CreateRaw(e, t), cancellationToken);
        }

        //without logging and exceptions
        protected abstract Task<T> CreateRaw(T entity, CancellationToken cancellationToken = default);

        protected async Task<IEnumerable<TResult>> ProcessAll<TInput, TResult>(string actionName,
            IEnumerable<TInput> input,
            ExecutionTypeSettings executionTypeSettings,
            Func<TInput, CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var inputArray = input.ToArray();
            var output = new TResult[inputArray.Length];
            var executionType = executionTypeSettings.ExecutionType == ExecutionType.Parallel ? "In Parallel" : "Sequentially";
            var stopwatch = new Stopwatch();
            try
            {
                Logger.Info("Start processing {action} for {entityName} collection {executionType}.",
                    actionName, typeof(T).Name, executionType);

                stopwatch.Start();

                switch (executionTypeSettings.ExecutionType)
                {
                    case ExecutionType.Sequent:
                        {
                            for (int i = 0; i < inputArray.Length; i++)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                try
                                {
                                    output[i] = await action(inputArray[i], cancellationToken);
                                }
                                catch (Exception e)
                                {
                                    Logger.Error(e, "Processing {action} for {entityName} {executionType} failed. {@entity}",
                                        actionName, typeof(T).Name, executionType, inputArray[i]);

                                    if (!executionTypeSettings.IgnoreErrorsForEntities)
                                        throw;
                                }
                            }
                            break;
                        }
                    case ExecutionType.Parallel:
                        {
                            // or should throw exception if fewer than 0
                            var threadsCount = executionTypeSettings.ParallelThreads <= 0
                                ? inputArray.Length
                                : executionTypeSettings.ParallelThreads;

                            var tasks = new Task[threadsCount];

                            int k = 0;
                            for (int i = 0; i < inputArray.Length; i++)
                            {
                                // multi threads issue
                                var tmpI = i;
                                cancellationToken.ThrowIfCancellationRequested();
                                tasks[k] = Task.Run(async () =>
                                {
                                    try
                                    {
                                        output[tmpI] = await action(inputArray[i], cancellationToken);
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Error(e, "Processing {action} for {entityName} {executionType} failed. {@entity}",
                                            actionName, typeof(T).Name, executionType, inputArray[i]);

                                        if (!executionTypeSettings.IgnoreErrorsForEntities)
                                            throw;
                                    }
                                });
                                if (k == tasks.Length - 1)
                                {
                                    // await for special count
                                    await Task.WhenAll(tasks);
                                    Array.Clear(tasks, 0, tasks.Length);
                                    k = 0;
                                }
                                k++;
                            }
                            // if last count of task a fewer that capacity (threads count)
                            if (k != 0)
                                await Task.WhenAll(tasks);
                            break;
                        }
                    default:
                        throw new NotSupportedException();
                }

                stopwatch.Stop();

                Logger.Info("Processing {action} for {entityName} {executionType} performed. Time elapsed {time}. Result: {$entities}",
                    actionName, typeof(T).Name, executionType, stopwatch.Elapsed, output);

                return output;

            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Logger.Error(e, "Processing {action} for {entityName} {executionType} failed. Time elapsed: {time}. Result: {$result}",
                    actionName, typeof(T).Name, executionType, stopwatch.Elapsed, output);
                if (!executionTypeSettings.IgnoreErrorsForExecution)
                    throw;
                return default;
            }
        }

        protected async Task ProcessAll<TInput>(string actionName,
            IEnumerable<TInput> input,
            ExecutionTypeSettings executionTypeSettings,
            Func<TInput, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            await ProcessAll(actionName, input, executionTypeSettings, async (e, t) =>
            {
                await action(e, t);
                return e;
            }, cancellationToken);
        }

        protected async Task<TResult> ProcessSingle<TInput, TResult>(string actionName,
            TInput input,
            Func<TInput, CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var stopwatch = new Stopwatch();
            try
            {
                Logger.Info("Start processing {action} for {entityName}.", actionName, typeof(T).Name);

                stopwatch.Start();
                var res = await action(input, cancellationToken);
                stopwatch.Stop();

                Logger.Info("Processing {action} for {entityName} performed. Time elapsed: {time}. Result: {@entity}",
                    actionName, typeof(T).Name, stopwatch.Elapsed, res);

                return res;
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Logger.Error(e, "Processing {action} for {entityName} failed. Time elapsed: {time}. {@entity}",
                    actionName, typeof(T).Name, stopwatch.Elapsed, input);
                throw;
            }
        }

        protected async Task ProcessSingle<TInput>(string actionName,
            TInput input,
            Func<TInput, CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            await ProcessSingle(actionName, input,  async (e, t) =>
            {
                await action(e, t);
                return e;
            }, cancellationToken);
        }
    }
}
