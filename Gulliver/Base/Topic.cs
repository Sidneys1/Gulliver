using System;
using System.Collections.Generic;
using ExtendedConsole;

namespace Gulliver.Base {
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    internal class HelpTopicAttribute : Attribute {
        public HelpTopicAttribute(string keyword, string category, string synopsis, string topicGetter = null, bool important = true) {
            Keyword = keyword;
            Category = category;
            Synopsis = synopsis;
            TopicGetter = topicGetter;
            Important = important;
        }

        public string Keyword { get; }
        public string Category { get; }
        public string Synopsis { get; }
        public string TopicGetter { get; }
        public bool Important { get; }
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

        private static readonly char[] Levels = {'#', '=', '~', '-'};
        private static bool _signalled;
        public void Print(bool full, int level = 0) {
            if (level == 0) _signalled = false;
            Console.WriteLine(Header);
            Console.WriteLine(new string(level < Levels.Length ? Levels[level] : Levels[Levels.Length - 1], Header.Length));
            Console.WriteLine();
            TopicBody.Write();
            Console.WriteLine();
            Console.WriteLine();
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