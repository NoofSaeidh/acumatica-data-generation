using DataGeneration.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Core.Extensions
{
    public static class SettingsExtensions
    {
        public static T ChangeSettings<T>(this T settings, Action<T> applier) where T : IGenerationSettings
        {
            if (applier is null)
                throw new ArgumentNullException(nameof(applier));

            applier(settings);
            return settings;
        }

        public static T SetCount<T>(this T settings, int count) where T : IGenerationSettings
        {
            settings.Count = count;
            return settings;
        }
    }
}
