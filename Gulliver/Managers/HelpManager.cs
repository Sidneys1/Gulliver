using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ExtendedConsole;
using Gulliver.Base;

namespace Gulliver.Managers {
    internal static class HelpManager {
        private static Dictionary<string, Topic> _topics;
        public static IReadOnlyDictionary<string, Topic> Topics => _topics;
        public static void Initialize() {
            _topics = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => Attribute.IsDefined((MemberInfo) t, typeof(HelpTopicAttribute)))
                .SelectMany(t => ((HelpTopicAttribute[])Attribute.GetCustomAttributes(t, typeof(HelpTopicAttribute))).Select(a => new Tuple<Type, HelpTopicAttribute>(t, a)))
                .ToDictionary(a => a.Item2.Keyword,
                    a => {
                        var att = a.Item2;
                        var type = a.Item1;
                        if (att.TopicGetter == null)
                            return new Topic(Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(att.Keyword), att.Category, att.Synopsis, att.Synopsis, essential: att.Important);
                        return (Topic)type.GetField(att.TopicGetter).GetValue(null);
                    });
        }

        public static void PrintTopicTree(bool all) {
            new Topic("Topics", null, "Use " + "help [TOPIC]".Cyan() + " to see more information on a single topic.", subHeaders: _topics.Values.Where(t => all || t.Essential).GroupBy(t => t.Category).OrderBy(g => g.Key).Select(g => new Topic(g.Key, null, FormattedString.Join("\n", g.Select(t => new FormattedString(
                $" * {t.Header}" + (t.Synopsis == null ? "" : $" - {t.Synopsis}"))))))).Print(true);
            if (!all && _topics.Values.Any(t => !t.Essential))
                ("More information available. Use ".Yellow() + "-f/--full".Cyan() + " to display.".Yellow() + "\n").Write();
        }
    }
}