using DataGeneration.Common;
using DataGeneration.Maint;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataGeneration
{
    public class ConsoleExecutor
    {
        #region Args

        public static class Args
        {
            public const string Config = "/config";
            public const string Settings = "/settings";
            public const string Start = "/start";
            public const string PutEndpoint = "/put-endpoint";
            public const string GetEndpoint = "/get-endpoint";
            public const string Help = "/?";
            internal const string _Default = "_default";

            public static readonly Dictionary<string, string> AvailableArgsHelp = new Dictionary<string, string>
            {
                [Start] = "Start generation. (Default - \"true\" if no args specified, \"false\" if any arg is specified)",
                [PutEndpoint] = "Put endpoint to Acumatica from file. If value not specified \"datagen-endpoint.xml\" file will be used.",
                [GetEndpoint] = "Get endpoint from Acumatica and save to the file. Values should be in following order {version} {endpoint} {filepath}",
                [Config] = "Specify json config file. If not specified default will be used.",
                //[Settings] = "Specify json files with single Generation Settings. All values will be merged to config's GenerationSettingsCollection. Can specify multiple.",
                [Help] = "Show this help."
            };
        }

        public const string GlobalHelp = "This tool for generation data via Contact-Based API to Acumatica. \r\n" +
            "Also you can use additional options presented bellow.";

        #endregion

        #region Console

        public const string ConfigFileName = "config.json";
        public const string EndpointDatagenFileName = "endpoint-datagen.xml";

        private Lazy<GeneratorConfig> _config = new Lazy<GeneratorConfig>(() => GeneratorConfig.ReadConfig(ConfigFileName));
        public GeneratorConfig Config => _config.Value;

        public static void WriteInfo(string line, ConsoleColor color = ConsoleColor.Gray, object exception = null)
        {
            Console.ForegroundColor = color;

            string getLine(string line_) => '|' + line_ + '|';

            var limitLine = getLine(new string('-', line.Length));

            Console.WriteLine();
            Console.WriteLine(limitLine);
            Console.WriteLine(getLine(line));
            Console.WriteLine(limitLine);
            Console.WriteLine();
            if (exception != null)
                Console.WriteLine(exception);

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        ///     Execute args (if any) and return value indicates to start datageneration or not (depending on args).
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Specifies to start datageneration.</returns>
        public bool ExecuteArgs(IEnumerable<string> args)
        {
            if (args.IsNullOrEmpty())
                return true;

            try
            {
                var result = new ArgsExecutor(args)
                    .Arg(Args.Help, ShowHelp, stopOnSuccess: true)
                    .Arg_ThrowNoValue(Args.Config, value => _config = new Lazy<GeneratorConfig>(() => GeneratorConfig.ReadConfig(value)))
                    //.Arg_ThrowNoValues(Args.Settings, values => Array.ForEach(values, v => Config.AddGenerationSettingsFromFile(v)))
                    .Arg(Args.PutEndpoint, PutEndpoint, () => PutEndpoint(EndpointDatagenFileName))
                    .Arg_ThrowNoValues(Args.GetEndpoint, GetAndSaveEndpoint)
                    .Arg(Args.Start, () => { }, res => res.StartGeneration = true)
                    .Default(WrongArgumentsSpecified)
                    .Result;

                if (result.StartGeneration.HasValue(out var startGeneration))
                    return startGeneration;

                if (result.Stopped == true)
                    return false;

                if (result.Executed.ContainsOnlyAnyOf(Args.Config, Args.Settings))
                    return true;

                return false;
            }
            catch (ArgsExecutionException aee)
            {
                WriteInfo(aee.Message, ConsoleColor.Red, aee);
            }
            catch (ArgsException ae)
            {
                WriteInfo(ae.Message, ConsoleColor.Red);
                ShowHelp();
            }
            catch (InvalidOperationException ioe)
            {
                WriteInfo(ioe.Message, ConsoleColor.Red, ioe);
            }
            catch (Exception e)
            {
                WriteInfo("Unexpected exception occurred.", ConsoleColor.Red, e);
            }
            return false;
        }

        private void ShowHelp()
        {
            WriteInfo("Help", ConsoleColor.Green);
            Console.WriteLine(GlobalHelp);
            Console.WriteLine();
            Console.WriteLine("Args:");
            Console.WriteLine();
            const int maxChars = 24;
            const int startSpacesCount = 4;
            var startSpaces = new string(' ', startSpacesCount);
            foreach (var arg in Args.AvailableArgsHelp)
            {
                var spacesCount = maxChars - arg.Key.Length - startSpacesCount;
                if (spacesCount <= 0)
                    spacesCount = 1;
                var spaces = new string(' ', spacesCount);
                Console.WriteLine(startSpaces + arg.Key + spaces + ":  " + arg.Value);
            }
        }

        private void WrongArgumentsSpecified()
        {
            WriteInfo("Wrong arguments specified.", ConsoleColor.Red);
            ShowHelp();
        }

        #endregion

        #region Execution

        public void PutEndpoint(string file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (!File.Exists(file)) throw new FileNotFoundException(nameof(file));

            using (var maint = MaintenanceClient.LoginLogoutClient(Config.ApiConnectionConfig))
            {
                maint.PutFileSchema(file);
            }

            Console.WriteLine($"Endpoint successfully updated from file {file}.");
        }

        private void GetAndSaveEndpoint(string[] args)
        {
            if (args.Length < 3)
                throw new InvalidOperationException("Not all arguments specified.");
            GetAndSaveEndpoint(args[0], args[1], args[2]);
        }
        public void GetAndSaveEndpoint(string version, string endpoint, string file)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (file == null) throw new ArgumentNullException(nameof(file));

            using (var maint = MaintenanceClient.LoginLogoutClient(Config.ApiConnectionConfig))
            {
                maint.GetAndSaveSchema(version, endpoint, file);
            }

            Console.WriteLine($"Endpoint {endpoint}/{version} successfully saved to file {file}.");
        }

        #endregion

        #region ArgsExecutor

        private class ArgsExecutor
        {
            private readonly Dictionary<string, string[]> _args;
            private bool _any;
            private bool _stopped;

            public ArgsExecutor(IEnumerable<string> args)
            {
                if (args == null)
                    throw new ArgumentNullException(nameof(args));

                // set key by order. 
                // if first is option, all next will be values until next option is found (starts with '/')
                string key = null;
                var groups = args
                    .Where(a => !string.IsNullOrWhiteSpace(a))
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
                    .GroupBy(a => a.key);

                var firstGroup = groups.FirstOrDefault();
                if (firstGroup == null)
                    throw new ArgsException("Options cannot be parsed. All of options are null or white spaces.");
                // args started not with option (/option)
                if (firstGroup.Key == null)
                    throw new ArgsException("Options must start from option, not a value.");

                _args = groups
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var vals = g
                                .Select(aa => aa.value)
                                .Where(aa => aa != null)
                                .ToArray();
                            if (!vals.Any())
                                return null;
                            return vals;
                        });

                Result = new ArgsExecutionResult();
            }

            // if some option caught
            public bool Any() => _any;

            public ArgsExecutor Arg(string arg, Action action, Action<ArgsExecutionResult> resulting = null, bool stopOnSuccess = false)
            {
                return Arg(arg, (Action<string[]>)null, action, resulting, stopOnSuccess);
            }

            // default action only if value is null
            public ArgsExecutor Arg(
                string arg,
                Action<string> action,
                Action defaultAction = null,
                Action<ArgsExecutionResult> resulting = null,
                bool stopOnSuccess = false)
            {
                return Arg(arg, (string[] s) => action(s[0]), defaultAction, resulting, stopOnSuccess);
            }

            public ArgsExecutor Arg_ThrowNoValue(
                string arg,
                Action<string> action,
                Action<ArgsExecutionResult> resulting = null,
                bool stopOnSuccess = false)
            {
                return Arg_ThrowNoValues(arg, (string[] s) => action(s[0]), resulting, stopOnSuccess);
            }

            public ArgsExecutor Arg(
                string arg,
                Action<string[]> action, Action
                defaultAction = null,
                Action<ArgsExecutionResult> resulting = null,
                bool stopOnSuccess = false)
            {
                if (Stopped) return this;

                if (action == null && defaultAction == null)
                    throw new InvalidOperationException("Both action and defaultAction cannot be null.");

                if (_args.TryGetValue(arg, out var value))
                {
                    var withValues = value != null;
                    try
                    {
                        if (withValues && action != null)
                        {
                            action(value);
                        }
                        else if (defaultAction != null)
                        {
                            defaultAction();
                        }
                        else return this;
                    }
                    catch (ArgsExecutionException) { throw; }
                    catch (Exception e)
                    {
                        throw new ArgsExecutionException($"Execution \"{arg}\" {(withValues ? "with" : "without")} values failed.", e);
                    }
                    _any = true;
                    Stopped |= stopOnSuccess;
                    resulting?.Invoke(Result);
                    Result.Executed.Add(arg);
                }
                return this;
            }

            public ArgsExecutor Arg_ThrowNoValues(
                string arg,
                Action<string[]> action,
                Action<ArgsExecutionResult> resulting = null,
                bool stopOnSuccess = false)
            {
                return Arg(arg, action, () => throw new ArgsExecutionException($"No value specified for option \"{arg}\"."), resulting, stopOnSuccess);
            }

            public ArgsExecutor Default(Action action, Action<ArgsExecutionResult> resulting = null)
            {
                if (!Stopped && !_any)
                {
                    action();
                    resulting?.Invoke(Result);
                    Result.Executed.Add(Args._Default);
                }
                return this;
            }

            public ArgsExecutionResult Result { get; }
            private bool Stopped
            {
                get => _stopped;
                set
                {
                    _stopped = value;
                    if (value)
                        Result.Stopped = value;
                }
            }
        }

        private class ArgsExecutionResult
        {
            public bool? StartGeneration { get; set; }
            public bool? Stopped { get; set; }
            public List<string> Executed { get; } = new List<string>();
        }

        private class ArgsExecutionException : ArgsException
        {
            public ArgsExecutionException(string message) : base(message)
            {

            }
            public ArgsExecutionException(string message, Exception innerException) : base(message, innerException)
            {

            }
        }

        private class ArgsException : Exception
        {
            public ArgsException(string message) : base(message)
            {
            }

            public ArgsException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
        #endregion
    }
}