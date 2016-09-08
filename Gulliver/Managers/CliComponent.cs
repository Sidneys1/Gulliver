using System;

namespace Gulliver.Managers {
    public abstract class CliComponent {
        public abstract void Initialize();
        public abstract void ProcessType(Type type);
    }
}