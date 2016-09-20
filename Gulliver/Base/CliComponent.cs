using System;

namespace Gulliver.Base {
    public abstract class CliComponent {
        public virtual void Initialize() { }
        public virtual void ProcessType(Type type) { }
    }
}