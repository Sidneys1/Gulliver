using System;
using ExtendedConsole;
using Gulliver.Base;
using SimpleArgv;

namespace Gulliver.Commands {
    [Command("rename")]
    [HelpTopic("rename", Topic.Commands, Summary, nameof(HelpTopic))]
    internal class RenameCommand : Command {
        #region Help TopicGetter

        private const string Summary = "Used to rename the active project.";

        public static readonly Topic HelpTopic = new Topic("Rename", Topic.Commands,
            Summary,
            Summary,
            subHeaders: new[] {
                new Topic("Usage", null,
                    "> " + "rename [NAME]".Cyan() + "",
                    subHeaders: new[] {
                        new Topic("Parameters", null,
                            " * " + "NAME: ".Cyan() + "The new name of the project.",
                            essential: false
                        )
                    }
                )
            }
        );

        #endregion

        private readonly CommandLine _parser = new CommandLine(new[] { "-", "--" });
        
        public RenameCommand() {
            _parser.AddArgument(strings => {
                var s = string.Join(" ", strings);
                if (string.IsNullOrWhiteSpace(s))
                    throw new ArgumentException("Parameter cannot be blank.", "NAME");
                return s;
            }, 
            string.Empty);
        }


        public override void Run(params string[] parameters) {
            GulliverCli.ProjectName = ((string) _parser.GetValue(string.Empty)).White();
        }
    }
}
