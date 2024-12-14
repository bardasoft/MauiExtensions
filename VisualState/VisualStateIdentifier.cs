using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public class VisualStateIdentifier
    {
        public string Name => (Identifer as VisualState)?.Name ?? Identifer as string ?? "";

        private readonly object Identifer;

        public VisualStateIdentifier(VisualState visualState) => Identifer = visualState;

        public VisualStateIdentifier(string visualStateName) => Identifer = visualStateName;

        public static implicit operator VisualStateIdentifier(VisualState visualState) => new VisualStateIdentifier(visualState);

        public static implicit operator VisualStateIdentifier(string visualStateName) => new VisualStateIdentifier(visualStateName);

        public VisualState GetVisualState(VisualElement context) => Identifer as VisualState ?? context.GetVisualStateByName((string)Identifer);
    }
}
