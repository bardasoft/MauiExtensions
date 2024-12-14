using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Extensions
{
    public class BehaviorFunc<T> : Behavior<T>
    where T : BindableObject
    {
        private Action<T> Action;

        public BehaviorFunc(Action<T> action)
        {
            Action = action;
        }

        protected override void OnAttachedTo(T bindable)
        {
            base.OnAttachedTo(bindable);
            Action(bindable);
        }
    }
}
