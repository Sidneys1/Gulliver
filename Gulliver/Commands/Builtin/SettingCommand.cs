﻿using System;
using System.Linq;
using System.Threading;
using ExtendedConsole;
using Gulliver.Base;
using Gulliver.Managers;
using Gulliver.Managers.Builtin;
using SimpleArgv;

namespace Gulliver.Commands.Builtin {
    [Command("setting", "settings")]
    internal class SettingCommand : Command {
        #region Help TopicGetter

        private const string Summary = "Lists or configures settings.";

        [HelpTopic("setting", "settings")]
        public static readonly Topic HelpTopic = new Topic("Setting", Topic.Commands,
            Summary,
            Summary,
            new[] {
                new Topic("Aliases", null,
                    "   * "+"setting\n".Cyan() +
                    "   * "+"settings".Cyan()
                ),
                new Topic("Usage", null,
                    "  > " + "setting [SETTING [-s/--set VALUE | -r/--reset]]".Cyan() + "",
                    subHeaders: new[] {
                        new Topic("Parameters", null,
                            "     * " + "SETTING".Cyan() + ": The setting to display or configure.\n" +
                            "     * " + "-s/--set VALUE".Cyan() + ": Sets SETTING to a new value.\n" +
                            "     * " + "-r/--reset".Cyan() + ": Resets SETTING to its default value.",
                            essential: false
                        )
                    }
                ),
                new Topic("Examples", null,
                    "   * List all settings:\n" +
                    "     > " + "settings\n".Cyan() +
                    "   * Reset the setting 'endian':\n" +
                    "     > " + "setting endian --reset\n".Cyan() +
                    "   * Set 'endian' to 'Little':\n" +
                    "     > " + "setting endian --set Little".Cyan() +
                    "",
                    essential: false
                ),
            }
        );

        #endregion

        #region TabCallback

        private static readonly string[] SetParam = { "--set", "-s" };
        private static readonly string[] ResetParam = { "--reset", "-r" };
        [TabCallback]
        public static string[] TabComplete(int index, string[] partials) {
            switch (index) {
                case 0:
                    return SettingsManager.Settings.Keys.Where(k => k.StartsWith(partials[index], StringComparison.OrdinalIgnoreCase)).ToArray();
                case 1:
                    return SetParam.Union(ResetParam).Where(i => i.StartsWith(partials[index], StringComparison.OrdinalIgnoreCase)).ToArray();
                case 2:
                    if (SetParam.Any(p => p.Equals(partials[1], StringComparison.OrdinalIgnoreCase))) {
                        var setting =
                            SettingsManager.Settings.Where(
                                k => k.Key.Equals(partials[0], StringComparison.OrdinalIgnoreCase)).Select(s => s.Value).ToArray();
                        if (setting.Length == 1 && setting[0].SettingType.IsEnum)
                            return Enum.GetValues(setting[0].SettingType).Cast<Enum>().Select(e => e.ToString()).Select(
                                s => {
                                    try {
                                        return setting[0].Validator.Invoke(s).ToString();
                                    } catch {
                                        return null;
                                    }
                                }).Where(v => v != null && v.StartsWith(partials[2], StringComparison.OrdinalIgnoreCase)).ToArray();
                    }
                    break;
            }
            return null;
        }

        #endregion

        private readonly CommandLine _parser = new CommandLine(new[] { "-", "--" });

        public SettingCommand() {
            _parser.AddArgument(s => {
                var j = string.Join(" ", s);
                if (string.IsNullOrWhiteSpace(j))
                    return null;
                if (!SettingsManager.Settings.ContainsKey(j))
                    throw new ArgumentException($"Setting '{j}' could not be found.", "SETTING");
                return j;
            }, string.Empty);
            _parser.AddArgument(s => string.Join(" ", s), SetParam);
            _parser.AddArgument(s => true, ResetParam);
        }

        public override void Run(params string[] parameters) {
            _parser.Parse(parameters);

            var setVal = _parser.GetValue<string>("--set", null);
            var reset = _parser.GetValue("--reset", false);
            if (setVal != null && reset)
                throw new ArgumentException("--set and --reset cannot be specified together.");

            var settingName = _parser.GetValue<string>(string.Empty, null);
            if (settingName == null) {
                Console.WriteLine();
                new Topic("Settings", null,
                    FormattedString.Join("\n",
                        SettingsManager.Settings.Select(
                            s => new FormattedString(" * ", s.Key.Yellow(), $": {s.Value.GetValue()}")))).Print(true);
                PrintHelp();
                return;
            }

            var setting = SettingsManager.Settings[settingName];
            if (reset) {
                setting.Reset();
                ("Setting '" + settingName.Yellow() + "' has been reset to '" + setting.GetValue().ToString() + "'\n").Write();
                Console.WriteLine();
                return;
            }
            if (setVal != null) {
                setting.SetValue(setVal);
                ("Setting '" + settingName.Yellow() + "' has been set to '" + setting.GetValue().ToString() + "'\n").Write();
                Console.WriteLine();
                return;
            }

            Console.WriteLine();

            var val = setting.GetValue();
            string[] pvals = null;
            if (val is Enum) {
                pvals = Enum.GetValues(val.GetType()).Cast<object>().Select(o => o.ToString()).ToArray();
            }

            new Topic(Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase("Setting '" + settingName + "'"), null,
                new FormattedString(
                    $"Value: {val}\n" +
                    $"Default: {setting.DefaultValue}"
                ) + (pvals == null ? "" : "\nPossible Values:\n * " + string.Join("\n * ", pvals))
            ).Print(true);
            PrintHelp(settingName);
        }

        private static void PrintHelp(string settingName = "SETTING") {
            ("Use " + "setting ".Cyan() + settingName.Yellow() + " --set [VALUE]".Cyan() + (settingName == "SETTING" ? " to change a setting.\n" : " to change this setting.\n"))
                .Write();
            ("Use " + "setting ".Cyan() + settingName.Yellow() + " --reset".Cyan() +
             (settingName == "SETTING" ? " to revert a setting to its default value.\n" : " to revert this setting to its default value.\n")).Write();
            Console.WriteLine();
        }
    }
}
