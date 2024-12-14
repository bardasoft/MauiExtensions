using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls
{
    public delegate Animation AnimationFactory(Action<double> callback, double start = 0, double end = 1, Easing easing = null, Action finished = null);

    /*public class AnimationFactory
    {
        private readonly Func<Action<double>, object, object, Easing, Action, Animation> Creator;

        public AnimationFactory(Func<Action<double>, object, object, Easing, Action, Animation> creator) => Creator = creator;

        public Animation Create(Action<double> callback, double start = 0, double end = 1, Easing easing = null, Action finished = null) => Creator(callback, start, end, easing, finished);
    }*/

    public class ExtendedAnimation : Animation
    {
        public ExtendedAnimation() : base() { }

        public ExtendedAnimation(Action<double> callback, double start = 0, double end = 1, Easing easing = null, Action started = null, Action finished = null) : base(value =>
        {
            if (started != null)
            {
                started.Invoke();
                started = null;
            }
            
            callback?.Invoke(value);

            if (value == end)
            {
                finished?.Invoke();
                finished = null;
            }
        }, start, end, easing) { }
    }

    public interface IStateTransitionManager
    {
        void StateWillChange(string oldState, string newState, Animation animation);

        void StateDidChange(string oldState, string newState);
    }

    public static class StateAnimation
    {
        public static Action<double> GetCallback(VisualElement visualElement, string endName, string startName = null, Action<double> callback = null)//, Easing easing = null, Action finished = null)
        {
            //var values = new Dictionary<BindableProperty, (object, object)>();

            //string startName = startState?.GetVisualState(visualElement)?.Name;
            //string endName = endState.GetVisualState(visualElement)?.Name;

            //List<Action<double>> childCallbacks = new List<Action<double>>(endingState.Setters.Count);
            //var startSetters = new List<(BindableObject, Setter)>();
            //var endSetters = new List<(BindableObject, Setter)>();

            /*Animation result = new ExtendedAnimation(callback, 0, 1, easing, startName == null ? null : new Action(() => visualElement.GoToState(startName)), () =>
            {
                visualElement.GoToState(endName);
                finished?.Invoke();
            });*/
            //Animation result = callback == null ? new Animation() : new Animation(callback);
            /*Animation result = new Animation(value =>
            {
                if (value == 0)
                {
                    VisualStateManagerExtensions.GoToState(startSetters);
                }

                callback?.Invoke(value);

                if (value == 1)
                {
                    VisualStateManagerExtensions.GoToState(endSetters);
                }
            }, 0, 1, easing, finished);*/

            /*IEnumerable<VisualElement> Targets()
            {
                yield return visualElement;

                foreach (VisualElement participant in visualElement.VisualStateParticipants(endName))
                {
                    yield return participant;
                }
            }*/

            Action<double> result = callback;

            var startStates = visualElement.GetAllAssociatedVisualStates(startName);

            foreach (var participant in visualElement.GetAllAssociatedVisualStates(endName))
            {
                VisualState end = participant.Item2;

                if (end == null)
                {
                    continue;
                }

                VisualState start = startStates.FirstOrDefault(value => value.Item1 == participant.Item1).Item2;

                int matched = 0;
                for (int i = 0; i < end.Setters.Count; i++)
                {
                    Setter endSetter = end.Setters[i];
                    Setter startSetter = null;

                    if (start != null)
                    {
                        for (int j = matched; j < start.Setters.Count; j++)
                        {
                            if (endSetter.Property == start.Setters[j].Property && endSetter.TargetName == start.Setters[j].TargetName)
                            {
                                startSetter = start.Setters[j];

                                if (j == matched)
                                {
                                    matched++;
                                }

                                break;
                            }
                        }
                    }

                    if (StateAnimationExtensions.Transition.ContainsKey(endSetter.Property.ReturnType))
                    {
                        result += PropertyAnimation.Create(participant.Item1, endSetter.Property, endSetter.Value, startSetter?.Value).GetCallback();
                    }
                    /*else
                    {
                        if (startSetter != null)
                        {
                            startSetters.Add((participant.Item1, startSetter));
                        }

                        (startSetter == null && endSetter.Value is BooleanValue ? startSetters : endSetters).Add((participant.Item1, endSetter));
                    }*/
                }
            }

            return result;
        }
    }
}
