using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ExtendedConsole;
using Gulliver.Base;

namespace Gulliver.Managers.Builtin {
    internal class HelpManager : CliComponent {
        private static readonly Dictionary<string, Tuple<Topic, bool>> _topics = new Dictionary<string, Tuple<Topic, bool>>(StringComparer.OrdinalIgnoreCase);
        public static IReadOnlyDictionary<string, Tuple<Topic, bool>> Topics => _topics;
        
        public override void ProcessType(Type type) {
            if (Attribute.IsDefined(type, typeof(AutoHelpTopicAttribute))) {
                foreach (var att in type.GetCustomAttributes<AutoHelpTopicAttribute>())
                {
                    var topic = new Topic(Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(att.Keywords[0]),
                        att.Category, att.Synopsis, att.Synopsis, essential: att.Important);
                    var keywords = att.Keywords;
                    _topics.Add(keywords[0], new Tuple<Topic, bool>(topic, false));
                    for (var index = 1; index < keywords.Length; index++)
                        _topics.Add(keywords[index], new Tuple<Topic, bool>(topic, true));
                }
            }

            foreach (var fieldInfo in 
                type.GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(f => Attribute.IsDefined((MemberInfo) f, typeof(HelpTopicAttribute)))) {
                var topic = fieldInfo.GetValue(null) as Topic;
                if (fieldInfo.FieldType != typeof(Topic)) continue;
                var keywords = fieldInfo.GetCustomAttribute<HelpTopicAttribute>().Keywords;
                _topics.Add(keywords[0], new Tuple<Topic, bool>(topic, false));
                for (var index = 1; index < keywords.Length; index++)
                    _topics.Add(keywords[index], new Tuple<Topic, bool>(topic, true));
            }
        }

        public static void PrintTopicTree(bool all) {
            new Topic("Topics", null,
                "Use " + "help [TOPIC]".Cyan() + " to see more information on a single topic.", 
                subHeaders: _topics.Values
                    .Where(t=>!t.Item2)
                    .Select(t=>t.Item1)
                    .Where(t => all || t.Essential)
                    .GroupBy(t => t.Category)
                    .OrderBy(g => g.Key)
                    .Select(g => new Topic(
                        g.Key, null,
                        FormattedString.Join("\n", 
                            g.Select(t => new FormattedString(
                                $" * {t.Header}" + (t.Synopsis == null ? string.Empty : $" - {t.Synopsis}")
                            ))
                        )
                    ))
            ).Print(true);

            if (!all && _topics.Values.Any(t => !t.Item1.Essential && !t.Item2))
                ("More information available. Use ".Yellow() + "-f/--full".Cyan() + " to display.".Yellow() + "\n").Write();
        }
    }
}