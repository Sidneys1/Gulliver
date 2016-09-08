using System;
using System.Collections.Generic;
using ExtendedConsole;

namespace Gulliver.Base {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    internal class AutoHelpTopicAttribute : HelpTopicAttribute {
        public AutoHelpTopicAttribute(string category, string synopsis, bool important, params string[] keywords) : base(keywords) {
            Category = category;
            Synopsis = synopsis;
            Important = important;
        }
        
        public string Category { get; }
        public string Synopsis { get; }
        public bool Important { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    internal class HelpTopicAttribute : Attribute {
        public HelpTopicAttribute(params string[] keywords) {
            Keywords = keywords;
        }

        public string[] Keywords { get; }
    }

    internal class Topic {
        public const string Commands = "Commands";
        public const string Settings = "Settings";

        public string Header { get; }
        public string Category { get; }
        public FormattedString TopicBody { get; }
        public IEnumerable<Topic> SubHeaders { get; }
        public bool Essential { get; }
        public string Synopsis { get; }

        public Topic(string header, string category, FormattedString topicBody, string synopsis = null, IEnumerable<Topic> subHeaders = null, bool essential = true) {
            Header = header;
            Category = category;
            TopicBody = topicBody;
            Synopsis = synopsis;
            SubHeaders = subHeaders;
            Essential = essential;
        }

        private static readonly char[] Levels = { '#', '=', '-' };
        private static bool _signalled;
        public void Print(bool full, int level = 0) {
            if (level == 0) _signalled = false;
            var space = new string(' ', level * 2);
            (space + Header + '\n' + space + new string(level < Levels.Length ? Levels[level] : Levels[Levels.Length - 1], Header.Length) + "\n").White().Write();
            TopicBody.Write();
            Console.WriteLine('\n');
            if (SubHeaders == null) return;
            foreach (var subHeader in SubHeaders) {
                if (!full && !subHeader.Essential) {
                    if (!_signalled) {
                        _signalled = true;
                        ("More information available. Use ".DarkGray() + "-f/--full".Cyan() + " to display.".DarkGray() + "\n").Write();
                    }
                    continue;
                }
                subHeader.Print(full, level + 1);
            }
        }
    }
}