using DataGeneration.Maint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataGeneration
{
    internal class Program
    {
        #region Execution

        private static void Main(string[] args)
        {
            ExecuteArgs(args, out var stopProcessing);
            if (stopProcessing)
                return;

            Generate(Config).Wait();
        }

        public static async Task Generate(GeneratorConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            using (var tokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    if (!tokenSource.IsCancellationRequested)
                        tokenSource.Cancel();
                    e.Cancel = true;
                };
                try
                {
                    await new GeneratorClient(config)
                        .GenerateAllOptions(tokenSource.Token)
                        .ConfigureAwait(false);

                    WriteInfo("Operation completed successfully.", ConsoleColor.Green);
                }
                catch (OperationCanceledException oce)
                {
                    WriteInfo("Operation has been canceled.", ConsoleColor.Red, oce);
                }
                catch (Exception e)
                {
                    WriteInfo("Unhandled exception has occurred.", ConsoleColor.Red, e);
                }
            }
        }

        #endregion

        #region Console

        public const string HelpArg = "/?";
        public const string StartArg = "/start";
        public const string UpdateEndpointArg = "/update-endpoint";
        public const string ConfigArg = "/config";

        public const string GlobalHelp = "This tool for generation data via Contact-Based API to Acumatica. \r\n" +
            "Also you can use additional options presented bellow.";


        private static readonly Dictionary<string, string> AvailableArgsHelp = new Dictionary<string, string>
        {
            [StartArg] = "Start generation. (Default - \"true\" if no args specified, \"false\" if any arg is specified)",
            [UpdateEndpointArg] = "Update endpoint. If value not specified \"datagen-endpoint.xml\" file will be used.",
            [ConfigArg] = "Specify config file. If not specified default will be used.",
            [HelpArg] = "Show this help."
        };

        #endregion

        #region Common

        public const string ConfigFileName = "config.json";
        public const string EndpointDatagenFileName = "endpoint-datagen.xml";

        private static Lazy<GeneratorConfig> _config = new Lazy<GeneratorConfig>(() => GeneratorConfig.ReadConfig(ConfigFileName));
        private static GeneratorConfig Config => _config.Value;

        // return true if contains any arg
        public static bool ExecuteArgs(string[] args, out bool stopProcessing)
        {
            var executer = new ArgsExecutor(args);
            if (executer.Empty())
                return stopProcessing = false;

            var stop = true;
            var res = executer
                .Arg(HelpArg, ShowHelp, stop: true)
                .Arg(ConfigArg, value => _config = new Lazy<GeneratorConfig>(() => GeneratorConfig.ReadConfig(value)), () => ThrowNoValueSpecified(ConfigArg))
                .Arg(UpdateEndpointArg, UpdateEndpoint, () => UpdateEndpoint(EndpointDatagenFileName))
                .Arg(StartArg, () => stop = false)
                .Default(WrongArgumentsSpecified)
                .Any();

            stopProcessing = stop;
            return res;
        }

        public static void ShowHelp()
        {
            WriteInfo("Help", ConsoleColor.Green);
            Console.WriteLine(GlobalHelp);
            Console.WriteLine();
            Console.WriteLine("Args:");
            Console.WriteLine();
            const int maxChars = 24;
            const int startSpacesCount = 4;
            var startSpaces = new string(' ', startSpacesCount);
            foreach (var arg in AvailableArgsHelp)
            {
                var spacesCount = maxChars - arg.Key.Length - startSpacesCount;
                if (spacesCount <= 0)
                    spacesCount = 1;
                var spaces = new string(' ', spacesCount);
                Console.WriteLine(startSpaces + arg.Key + spaces + ":  " + arg.Value);
            }
        }

        public static void WrongArgumentsSpecified()
        {
            WriteInfo("Wrong arguments specified.", ConsoleColor.Red);
            ShowHelp();
        }

        public static void UpdateEndpoint(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (!File.Exists(file)) throw new FileNotFoundException(nameof(file));

            using (var maint = MaintenanceClient.LoginLogoutClient(Config.ApiConnectionConfig))
            {
                maint.PutFileSchema(file);
            }
        }

        private static void ThrowNoValueSpecified(string option)
        {
            throw new InvalidOperationException($"No value specified for option \"{option}\".");
        }

        private static void WriteInfo(string line, ConsoleColor color = ConsoleColor.Gray, Exception e = null)
        {
            Console.ForegroundColor = color;

            string getLine(string line_) => '|' + line_ + '|';

            var limitLine = getLine(new string('-', line.Length));

            Console.WriteLine();
            Console.WriteLine(limitLine);
            Console.WriteLine(getLine(line));
            Console.WriteLine(limitLine);
            Console.WriteLine();
            if (e != null)
                Console.WriteLine(e);

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        #endregion

        #region ArgsExecutor

        internal class ArgsExecutor
        {
            private readonly Dictionary<string, string[]> _args;
            private bool _any;
            private bool _stopped;
            public ArgsExecutor(IEnumerable<string> args)
            {
                string key = null;
                _args = args?.Where(a => !string.IsNullOrWhiteSpace(a))
                    .Select(a =>
                    {
                        var arg = a.Trim();
                        string value = null;
                        if (arg.StartsWith("/"))
                        {
                            key = arg;
                        }
                        else
                        {
                            value = arg;
                        }
                        return new { key, value };
                    })
                    .GroupBy(a => a.key)
                    .ToDictionary(
                        a => a.Key,
                        a =>
                        {
                            var vals = a
                                .Select(aa => aa.value)
                                .Where(aa => aa != null)
                                .ToArray();
                            if (!vals.Any())
                                return null;
                            return vals;
                        });
            }

            // if some option caught
            public bool Any() => _any;
            public ArgsExecutor Arg(string arg, Action action, bool stop = false)
            {
                if (_stopped) return this;

                if (_args.ContainsKey(arg))
                {
                    action();
                    _any = true;
                    _stopped |= stop;
                }
                return this;
            }
            // default action only if value is null
            public ArgsExecutor Arg(string arg, Action<string> action, Action defaultAction = null, bool stop = false)
            {
                if (_stopped) return this;

                if (_args.TryGetValue(arg, out var value))
                {
                    if (value != null)
                    {
                        action(value[0]);
                        _any = true;
                    }
                    else if (defaultAction != null)
                    {
                        defaultAction();
                        _any = true;
                    }
                    _stopped |= stop;
                }
                return this;
            }
            // default action only if value is null
            public ArgsExecutor Arg(string arg, Action<string[]> action, Action defaultAction = null, bool stop = false)
            {
                if (_stopped) return this;

                if (_args.TryGetValue(arg, out var value) && value.Length > 0)
                {
                    if (value != null)
                    {
                        action(value);
                        _any = true;
                    }
                    else if (defaultAction != null)
                    {
                        defaultAction();
                        _any = true;
                    }
                    _stopped |= stop;
                }
                return this;
            }

            public ArgsExecutor Default(Action action)
            {
                if (!_stopped && !_any)
                    action();
                return this;
            }
            // if no options
            public bool Empty() => _args == null || !_args.Any();
        }

        #endregion
    }
}