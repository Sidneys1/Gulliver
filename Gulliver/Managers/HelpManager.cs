using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ExtendedConsole;
using Gulliver.Base;

namespace Gulliver.Managers {
    internal class HelpManager : CliComponent {
        private static Dictionary<string, Topic> _topics;
        public static IReadOnlyDictionary<string, Topic> Topics => _topics;
        public override void Initialize() {
            _topics = new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase);
        }

        public override void ProcessType(Type type) {
            if (Attribute.IsDefined(type, typeof(AutoHelpTopicAttribute))) {
                foreach (var att in type.GetCustomAttributes<AutoHelpTopicAttribute>()) {
                    var topic = new Topic(Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(att.Keywords[0]),
                        att.Category, att.Synopsis, att.Synopsis, essential: att.Important);
                    foreach (var keyword in att.Keywords)
                        _topics.Add(keyword, topic);
                }
            }

            foreach (var fieldInfo in 
                type.GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(f => Attribute.IsDefined(f, typeof(HelpTopicAttribute)))) {
                var topic = fieldInfo.GetValue(null) as Topic;
                if (fieldInfo.FieldType != typeof(Topic)) continue;
                foreach (var keyword in fieldInfo.GetCustomAttribute<HelpTopicAttribute>().Keywords)
                    _topics.Add(keyword, topic);
            }
        }

        public static void PrintTopicTree(bool all) {
            new Topic("Topics", null, "Use " + "help [TOPIC]".Cyan() + " to see more information on a single topic.", subHeaders: _topics.Values.Where(t => all || t.Essential).GroupBy(t => t.Category).OrderBy(g => g.Key).Select(g => new Topic(g.Key, null, FormattedString.Join("\n", g.Select(t => new FormattedString(
                $" * {t.Header}" + (t.Synopsis == null ? "" : $" - {t.Synopsis}"))))))).Print(true);
            if (!all && _topics.Values.Any(t => !t.Essential))
                ("More information available. Use ".Yellow() + "-f/--full".Cyan() + " to display.".Yellow() + "\n").Write();
        }
    }
}