using DataGeneration.Core;
using DataGeneration.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataGeneration.GenerationInfo
{
    public class GenerationSubscriptionSettings
    {
        public bool AddTelemetryMarkers { get; set; }
        // inject into search settings batches start time and end time
        // GenerationSettings.StartTime -> ISearchUtilizer.SearchPattern.CreatedDate.Injected.Value
        //public bool AddBatchTimeToSearches { get; set; }

        public GenerationSubscriptionManager GetSubscriptionManager(GeneratorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var manager = new GenerationSubscriptionManager();
            if (AddTelemetryMarkers) AddTelemetryEvents(config, manager);
            return manager;
        }

        protected void AddTelemetryEvents(GeneratorConfig config, GenerationSubscriptionManager manager)
        {
            var sender = new TelemetryMarkerSender(config.ApiConnectionConfig.EndpointSettings.TelemetryMarkerUrl);
            manager.Add(
                async (s, e) => await sender.SendHttpMark(GetArgs("BeforeGeneration", e.GenerationSettings)),
                async (s, e) => await sender.SendHttpMark(GetArgs("Generation", e.GenerationSettings)),
                async (s, e) => await sender.SendHttpMark(("EventType", "null"))
            );
        }

        private (string, string)[] GetArgs(string eventType, IGenerationSettings generationSettings)
        {
            var threads = generationSettings.ExecutionTypeSettings.ExecutionType == ExecutionType.Sequent
                ? 1
                : generationSettings.ExecutionTypeSettings.ParallelThreads;
            return new (string, string)[] {
                ("EventType", eventType),
                ("GenType", generationSettings.GenerationType),
                ("Count", generationSettings.Count.ToString()),
                ("Threads", threads.ToString())
            };
        }

        private class TelemetryMarkerSender
        {
            private readonly string _markerUrl;
            private readonly Regex _replaceWhitespaces = new Regex("\\s", RegexOptions.Compiled);
            public TelemetryMarkerSender(string markerUrl)
            {
                _markerUrl = markerUrl ?? throw new ArgumentNullException(nameof(markerUrl));
            }

            public async Task SendHttpMark(params (string key, string value)[] args)
            {
                using (var http = new HttpClient())
                {
                    await http.GetAsync(GetUrl(args), HttpCompletionOption.ResponseHeadersRead);
                }
            }

            private string GetUrl(params (string key, string value)[] args)
            {
                var sb = new StringBuilder();
                sb.Append(_markerUrl);
                foreach (var item in args.Select(a => GetSingleMark(a)))
                {
                    sb.Append(item);
                }
                return sb.ToString();
            }

            private string GetSingleMark((string key, string value) arg)
            {
                return $"&{_replaceWhitespaces.Replace(arg.key, "_")}={_replaceWhitespaces.Replace(arg.value, "_")}";
            }
        }
    }
}
