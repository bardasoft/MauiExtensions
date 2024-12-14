using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;

using Constructor = System.Func<double[], object>;
using Deconstructor = System.Func<object, double[]>;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
    using DoubleExtractor = Tuple<Deconstructor, Constructor>;

    public static class VisualStateManagerExtensions
    {
        public static void SetValue(this VisualElement visualElement, BindableProperty property, object value, string name, VisualElement stateOwner = null)
        {
            Setter setter = GetSetter(visualElement, property, name, stateOwner);

            if (setter != null)
            {
                return;
            }

            setter.Value = value;

            VisualStateGroup group;
            VisualState state;
            GetVisualStateInfo(visualElement, name, out group, out state);

            if (group.CurrentState == state)
            {
                visualElement.SetValue(setter.Property, setter.Value);
            }
        }

        public static object GetValue(this VisualElement visualElement, BindableProperty property, string name, VisualElement stateOwner = null) => GetSetter(visualElement, property, name, stateOwner)?.Value ?? property.DefaultValue;

        private static Setter GetSetter(this VisualElement visualElement, BindableProperty property, string name, VisualElement stateOwner = null)
        {
            if (stateOwner == null)
            {
                stateOwner = visualElement;
            }

            //VisualState visualState = visualStateIdentifier.GetVisualState(visualElement);
            //name = visualElement.GetParticipantStateName(name, stateOwner);
            foreach (VisualState vs in ParentStates(visualElement, name))
            {
                VisualState state = visualElement == stateOwner ? vs : visualElement.GetVisualStateByName(vs.GetId());

                foreach (Setter setter in state.Setters)
                {
                    if (setter.Property == property)// && (setter.GetTarget() ?? visualElement) == visualElement)
                    {
                        return setter;
                    }
                }
            }

            return null;
        }

        //private static string GetParticipantStateName(this VisualElement visualElement, string name, VisualElement stateOwner) => stateOwner == null || stateOwner == visualElement ? name : visualElement.GetVisualStateByName(name).GetId();

        //public static void SetValueFromState(this VisualElement visualElement, BindableProperty property, VisualElement owner, string name) => visualElement.SetValue(property, visualElement.GetValue(property, owner, name));

        public static IEnumerable<(VisualElement, VisualState)> GetAllAssociatedVisualStates(this VisualElement visualElement, string name)
        {
            string participantName = visualElement.GetVisualStateByName(name)?.GetId();

            foreach (VisualElement participant in GetVisualStateParticipants(visualElement, name))
            {
                string temp = participant == visualElement ? name : participantName;
                yield return (participant, participant.GetVisualStateByName(temp));
            }
        }

        public static IEnumerable<VisualElement> GetVisualStateParticipants(this VisualElement visualElement, string stateName)
        {
            VisualState key = visualElement.GetVisualStateByName(stateName);

            ISet<VisualElement> value;
            if (key != null && Lookup.TryGetValue(key, out value))
            {
                //return value.Prepend(visualElement);

                yield return visualElement;

                foreach (var participant in value)
                {
                    yield return participant;
                }
            }
        }

        private static IEnumerable<VisualState> ParentStates(VisualElement visualElement, string name)
        {
            while (true)
            {
                VisualStateGroup group;
                VisualState state;
                GetVisualStateInfo(visualElement, name, out group, out state);

                if (state == null)
                {
                    break;
                }

                name = group.Name;
                yield return state;
            }
        }

        public static void SetCurrentState(this VisualElement visualElement, string name)
        {
            VisualStateGroup group = VisualStateManager.GetVisualStateGroups(visualElement)[0];
            VisualState state = new VisualState { Name = name };

            group.States.Add(state);
            VisualStateManager.GoToState(visualElement, name);
            group.States.Remove(state);
        }

        public static VisualState CurrentState(this VisualElement visualElement, VisualStateGroup group = null)
        {
            IList<VisualStateGroup> groups = VisualStateManager.GetVisualStateGroups(visualElement);

            if (group == null)
            {
                if (!VisualStateManager.HasVisualStateGroups(visualElement))
                {
                    return null;
                }

                group = groups[0];
            }

            VisualStateGroup child = group.CurrentState == null ? null : groups.FirstOrDefault(vgs => vgs.Name == group.CurrentState.Name && vgs.States.Contains(vgs.CurrentState));

            return child == null ? group.CurrentState : CurrentState(visualElement, child);
        }

        //private static readonly VisualState EmptyState = new VisualState { Name = "null" };

        private static void Unapply(VisualElement visualElement, string name)
        {
            VisualStateGroup group;
            VisualState state;
            GetVisualStateInfo(visualElement, name, out group, out state);

            if (state == null || group.CurrentState != state)
            {
                return;
            }

            string participantGroupName = group?.GetId();
            foreach (VisualElement participant in visualElement.GetVisualStateParticipants(state.Name))
            {
                VisualStateGroup parent = VisualStateManager.GetVisualStateGroups(participant).FirstOrDefault(vgs => vgs.Name == (participant == visualElement ? group.Name : participantGroupName));

                VisualState temp = new VisualState { Name = parent.GetHashCode().ToString() };
                parent.States.Add(temp);
                VisualStateManager.GoToState(participant, temp.Name);
                parent.States.Remove(temp);
            }

            Unapply(visualElement, group.Name);
        }

        private static readonly BindableProperty StateTransitionManagerProperty = BindableProperty.CreateAttached("StateTransitionManager", typeof(IStateTransitionManager), typeof(VisualElement), null);

        public static IStateTransitionManager GetStateTransitionManager(VisualElement visualElement) => (IStateTransitionManager)visualElement.GetValue(StateTransitionManagerProperty) ?? visualElement as IStateTransitionManager;
        public static void SetStateTransitionManager(VisualElement visualElement, IStateTransitionManager manager) => visualElement.SetValue(StateTransitionManagerProperty, manager);

        public static string GoToState(this VisualElement visualElement, string name, uint rate = 16, uint length = 250, Easing easing = null, Action<double> callback = null, Action<double, bool> finished = null, Func<bool> repeat = null)
        {
            string currentState = visualElement.CurrentState()?.Name;
            if (currentState == name)
            {
                return null;
            }

            IStateTransitionManager manager = GetStateTransitionManager(visualElement);

            //Animation animation = StateAnimation.GetCallback(visualElement, name, callback: callback);
            Animation animation = new Animation(StateAnimation.GetCallback(visualElement, name, callback: callback));
            manager?.StateWillChange(currentState, name, animation);

            string handle = name + "StateTransition";
            animation.Commit(visualElement, handle, rate, length, easing, (final, canceled) =>
            {
                BatchBegin(visualElement, currentState, name);

                Apply(visualElement, visualElement.GetVisualStateByName(name));
                manager?.StateDidChange(currentState, name);

                BatchCommit(visualElement, currentState, name);

                finished?.Invoke(final, canceled);
            }, repeat);

            return handle;
        }

        public static bool GoToState(this VisualElement visualElement, string name)
        {
            string currentState = visualElement.CurrentState()?.Name;
            if (currentState == name)
            {
                return false;
            }

            IStateTransitionManager manager = GetStateTransitionManager(visualElement);

            BatchBegin(visualElement, currentState, name);
            
            manager?.StateWillChange(currentState, name, null);
            bool result = Apply(visualElement, visualElement.GetVisualStateByName(name));
            manager?.StateDidChange(currentState, name);
            
            BatchCommit(visualElement, currentState, name);

            return result;

            /*Print.Log(visualElement + " going to state", name);
            visualElement.PrettyPrintStates();
            Print.Log("------------------------------");*/


            //animation?.Commit(visualElement, currentState + "-->" + name);

            //Print.Log("------------------------------\n");
        }

        private static void BatchBegin(VisualElement visualElement, string from, string to) => Batch(visualElement, from, to, v => v.BatchBegin());
        private static void BatchCommit(VisualElement visualElement, string from, string to) => Batch(visualElement, from, to, v => v.BatchCommit());

        private static void Batch(VisualElement visualElement, string from, string to, Action<VisualElement> action)
        {
            foreach (string name in new string[] { from, to })
            {
                foreach (VisualState state in ParentStates(visualElement, name))
                {
                    foreach (VisualElement participant in GetVisualStateParticipants(visualElement, state.Name))
                    {
                        action(participant);
                    }
                }
            }
        }

        internal static bool Apply(this VisualElement visualElement, VisualState state)
        {
            VisualStateGroup group;
            GetVisualStateInfo(visualElement, state.Name, out group, out state);

            VisualState current = visualElement.CurrentState(group);
            if (current == state)
            {
                return true;
            }
            else if (current != null)
            {
                Unapply(visualElement, current.Name);
            }

            bool result = true;

            VisualState parent = visualElement.GetVisualStateByName(group?.Name);
            if (parent != null)
            {
                result = Apply(visualElement, parent);
            }
            //Print.Log("going to " + name + " from " + group.CurrentState?.Name);
            //result |= VisualStateManager.GoToState(visualElement, state.Name);

            string childStateName = state?.GetId();

            foreach (VisualElement participant in visualElement.GetVisualStateParticipants(state.Name))
            {
                if (participant == visualElement)
                {
                    //continue;
                }

                result |= VisualStateManager.GoToState(participant, participant == visualElement ? state.Name : childStateName);
            }

            return result;
        }

#if DEBUG
        public static void PrettyPrintStates(this VisualElement visualElement)
        {
            foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(visualElement))
            {
                Print.Log(group.Name);

                foreach (VisualState state in group.States)
                {
                    Print.Log("\t" + state.Name + (state == group.CurrentState ? "*" : ""));
                }
            }
        }
#endif

        public static bool GoToState(this VisualElement visualElement, string name, VisualElement stateOwner)
        {
            /*VisualState state = stateOwner.GetVisualStateByName(name);

            if (state == null)
            {
                return false;
            }*/

            if (visualElement != stateOwner)
            {
                name = stateOwner.GetVisualStateByName(name)?.GetId();
            }

            foreach (VisualState state in ParentStates(visualElement, name).Reverse())
            {
                foreach (Setter setter in state.Setters)
                {
                    if (setter.Value is Binding binding)
                    {
                        visualElement.SetBinding(setter.Property, binding);
                    }
                    else
                    {
                        visualElement.SetValue(setter.Property, setter.Value);
                    }
                }
            }

            return true;
        }

        public static void GoToState(IList<(BindableObject, Setter)> setters)
        {
            foreach (var setter in setters)
            {
                setter.Item1.SetValue(setter.Item2.Property, setter.Item2.Value);
            }
        }

        /*public static void AddVisualStateGroup(this VisualElement owner, VisualStateGroup group) => AddVisualStateGroup(owner, owner, group);

        public static void AddVisualStateGroup(this VisualElement owner, VisualElement target, VisualStateGroup group)
        {
            VisualStateManager.GetVisualStateGroups(target).Add(group);

            foreach (VisualState state in group.States)
            {
                owner.AddVisualStateParticipant(state.Name, target);
            }
        }*/

        private static IDictionary<VisualState, ISet<VisualElement>> Lookup = new Dictionary<VisualState, ISet<VisualElement>>();

        private static string GetLookupKey(this VisualState state) => state.Name;

        private static string GetId(this VisualStateGroup group) => group.Name + "." + group.GetHashCode().ToString();

        private static string GetId(this VisualState state) => state.Name + "." + state.GetHashCode().ToString();

        private static bool TryGet<T>(this IList<T> list, int index, out T value)
        {
            if (index < list.Count && index > 0)
            {
                value = list[index];
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static void SetVisualStateValues(this VisualElement target, VisualStateValues values)
        {
            VisualElement owner = values.Owner ?? target;

            foreach (VisualStateGroup ownerGroup in VisualStateManager.GetVisualStateGroups(owner))
            {
                foreach (VisualState ownerState in ownerGroup.States)
                {
                    object value;
                    if (!values.TryGetValue(ownerState.Name, out value))
                    {
                        continue;
                    }

                    //string key = state.GetLookupKey();
                    ISet<VisualElement> participants;
                    if (!Lookup.TryGetValue(ownerState, out participants))
                    {
                        Lookup[ownerState] = participants = new HashSet<VisualElement>();
                    }

                    VisualState state;

                    if (target == owner)
                    {
                        state = ownerState;
                    }
                    else
                    {
                        participants.Add(target);

                        string groupName = ownerGroup.GetId();
                        VisualStateGroup group = VisualStateManager.GetVisualStateGroups(target).FirstOrDefault(vsg => vsg.Name == groupName);

                        if (group == null)
                        {
                            VisualStateManager.GetVisualStateGroups(target).Add(group = new VisualStateGroup { Name = groupName });
                        }

                        string stateName = ownerState.GetId();
                        state = group.States.FirstOrDefault(vs => vs.Name == stateName);

                        if (state == null)
                        {
                            group.States.Add(state = new VisualState { Name = stateName });
                        }
                    }

                    state.Setters.Add(new Setter
                    {
                        Property = values.Property,
                        Value = value
                    });
                }
            }
        }

        private static bool GetVisualStateInfo(VisualElement visualElement, string name, out VisualStateGroup group, out VisualState state)
        {
            foreach (VisualStateGroup vsg in VisualStateManager.GetVisualStateGroups(visualElement))
            {
                foreach (VisualState vs in vsg.States)
                {
                    if (vs.Name == name)
                    {
                        group = vsg;
                        state = vs;

                        return true;
                    }
                }
            }

            group = null;
            state = null;
            return false;
        }
    }

    public static class XamlExtensions
    {
        private static readonly OnPlatformExtension Converter = new OnPlatformExtension();

        public static object Convert(object value, IServiceProvider serviceProvider)
        {
            Converter.Default = value;
            return Converter.ProvideValue(serviceProvider);
        }
    }

    public class VisualStates : List<VisualState>
    {
        public VisualStates(string state1, string state2, params string[] moreStates) : this(() => new VisualState(), state1, state2, moreStates) { }

        public VisualStates() { }

        public VisualStates(Func<VisualState> loadTemplate, string state1, string state2, params string[] moreStates) : this(loadTemplate(), loadTemplate(), Create(moreStates.Length, loadTemplate))
        {
            this[0].Name = state1;
            this[1].Name = state2;
            for (int i = 0; i < moreStates.Length; i++)
            {
                this[i + 2].Name = moreStates[i];
            }
        }

        public VisualStates(VisualState state1, VisualState state2, params VisualState[] moreStates) : base(2 + moreStates.Length)
        {
            (this).Add(state1, state2);
            AddRange(moreStates);
        }

        public static implicit operator VisualStateGroup(VisualStates visualStates)
        {
            VisualStateGroup result = new VisualStateGroup { Name = visualStates[0].Name };
            result.States.AddRange(visualStates);
            return result;
        }

        private static VisualState[] Create(int count, Func<VisualState> loadTemplate)
        {
            VisualState[] result = new VisualState[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = loadTemplate();
            }
            return result;
        }

        public void Add(SetterTemplate setterTemplate, params (string, object)[] values)
        {
            foreach (var value in values)
            {
                Setter setter = setterTemplate.Create();
                setter.Value = value.Item2;
                (this).GetVisualStateByName(value.Item1).Setters.Add(setter);
            }
        }
    }

    public static class MultiVisualStateExtensions
    {
        //public static void Add(this VisualStateGroupList visualStateGroups, VisualStates states) => visualStateGroups.Add(states);

        public static void Add<T>(this VisualStates visualState, SetterTemplate setterTemplate, params T[] values)
        {
            int i = 0;
            foreach (var value in values)
            {
                Setter setter = setterTemplate.Create();
                setter.Value = value;
                visualState[i++].Setters.Add(setter);
            }
        }

        public static void Add(this IList<VisualState> visualStates, TargetedSetters setters)
        {
            int i = 0;
            foreach (Setter setter in setters)
            {
                visualStates[(i++ % visualStates.Count)].Setters.Add(setter);
            }
        }
    }

    public class SetterTemplate
    {
        protected readonly Func<Setter> LoadTemplate;

        public SetterTemplate() { }

        public SetterTemplate(Func<Setter> loadTemplate) => LoadTemplate = loadTemplate;

        public static implicit operator SetterTemplate(BindableProperty property) => new SetterTemplate(() => new Setter { Property = property });

        public Setter Create() => LoadTemplate?.Invoke() ?? new Setter();
    }

    public class TargetedSetters : IEnumerable<Setter>
    {
        public List Targets { get; set; }
        public List<Setters> Setters { get; } = new List<Setters>();

        public TargetedSetters() => Targets = new List();
        public TargetedSetters(params BindableObject[] targets) => Targets = new List(targets);

        public static implicit operator TargetedSetters(Setters setters) => new TargetedSetters { Setters = { setters } };

        public void Add(Setters setter) => Setters.Add(setter);

        public IEnumerator<Setter> GetEnumerator()
        {
            foreach (BindableObject target in Targets.Count == 0 ? new List { null } : Targets)
            {
                foreach (Setters setterProperty in Setters)
                {
                    foreach (Setter setter in setterProperty)
                    {
                        if (target != null)
                        {
                            setter.Value = new UniversalSetter.Pair { Target = target, Value = setter.Value };
                        }

                        yield return setter;
                    }
                }
            }
        }

        public class List : List<BindableObject>
        {
            public List() : base() { }

            public List(IEnumerable<BindableObject> collection) : base(collection) { }

            public static implicit operator List(BindableObject bindable) => new List { bindable };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [ContentProperty("Values")]
    public class Setters : IEnumerable<Setter>
    {
        public BindableProperty Property { get; set; }
        public List<object> Values { get; } = new List<object>();

        private readonly SetterTemplate LoadTemplate;

        public Setters() : this(new SetterTemplate()) { }

        public Setters(BindableProperty property) : this() => Property = property;

        public Setters(SetterTemplate loadTemplate) => LoadTemplate = loadTemplate;

        public void Add(object value) => Values.Add(value);

        public IEnumerator<Setter> GetEnumerator()
        {
            foreach (object value in Values)
            {
                Setter setter = LoadTemplate.Create();
                if (Property != null)
                {
                    setter.Property = Property;
                }
                setter.Value = value;

                yield return setter;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public struct AnalogBool
    {
        public double Value { get; set; }
        public bool IntermediateValue { get; set; }

        public static implicit operator bool(AnalogBool analogBool) => analogBool.Value == 0 ? false : (analogBool.Value == 1 ? true : analogBool.IntermediateValue);
    }

    public class Thunk<T>
    {
        public T Value => Evaluator();

        private readonly Func<T> Evaluator;

        public Thunk(Func<T> evaluator) => Evaluator = evaluator;

        public static implicit operator T(Thunk<T> thunk) => thunk.Value;
    }

    public abstract class BooleanValue { }

    public class BooleanValue<T> : BooleanValue
    {
        public object Value { get; set; }

        public BooleanValue() { }

        public BooleanValue(T value) => Value = value;

        public static explicit operator BooleanValue<T>(T value) => new BooleanValue<T>(value);

        public static explicit operator BooleanValue<T>(Thunk<T> value) => new BooleanValue<T>(value);

        public static implicit operator T(BooleanValue<T> booleanValue) => (T)(dynamic)booleanValue.Value;
    }

    /*public class TargetSetter : IEnumerable<Setters>
    {
        public BindableObject Target { get; set; }
        public List<Setters> Setters { get; } = new List<Setters>();

        /*public class List : IEnumerable<MultiSetter>
        {
            private TargetSetter Context;
            private List<MultiSetter> Setters = new List<MultiSetter>();

            public List(TargetSetter context) => Context = context;

            public void Add<T>(MultiSetter<T> setter)
            {
                setter.Target = Context.Target;
                Setters.Add(setter);
            }

            public IEnumerator<MultiSetter> GetEnumerator() => Setters.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public IEnumerator<Setters> GetEnumerator()
        {
            foreach (Setters setter in Setters)
            {
                setter.Target = Target;
                yield return setter;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }*/

    /*public class MultiSetter<T> : IEnumerable<Setter>
    {
        public BindableObject Target { get; set; }
        public BindableProperty Property { get; set; }
        public List<object> Values { get; } = new List<object>();

        public static implicit operator MultiSetter(MultiSetter<T> setter)
        {
            MultiSetter result = new MultiSetter { Property = setter.Property };

            foreach (object value in setter.Values)
            {
                result.Values.Add(new BindingTargetValuePair<T>(setter.Target, value));
            }

            return result;
        }

        public IEnumerator<Setter> GetEnumerator()
        {
            foreach(object value in Values)
            {
                yield return new Setter<T> { Target = Target, Property = Property, Value = value };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }*/

    public class UniversalSetter
    {
        public BindableObject Target { get; set; }
        public BindableProperty Property { get; set; }
        public object Value { get; set; }

        public static implicit operator Setter(UniversalSetter setter) => new Setter { Property = setter.Property, Value = new Pair { Target = setter.Target, Value = setter.Value } };

        public class Pair
        {
            public BindableObject Target { get; set; }
            public object Value { get; set; }
        }
    }

    /*public class BindingTargetValuePair<T> : BindingTargetValuePair
    {
        public BindingTargetValuePair(BindableObject target, object value)
        {
            Target = target;
            Value = value;
        }

        //public static implicit operator T(BindingTargetValuePair<T> test) => (T)(dynamic)test.Value;
    }*/

    public class BindingPair
    {
        private BindableObject Target;
        private BindableProperty Property;

        public BindingPair(BindableObject target, BindableProperty property)
        {
            Target = target;
            Property = property;
        }

        public static implicit operator BindableProperty(BindingPair bindingPair) => bindingPair.Property;
    }

    public static class StateAnimationExtensions
    {
        public static Dictionary<Type, DoubleExtractor> Transition = new Dictionary<Type, DoubleExtractor>();

        //public static BindableProperty EnteredStateProperty = BindableProperty.CreateAttached("EnteredState", typeof(Action), typeof(IAnimatable), null, propertyChanged: (bindable, oldValue, newValue) => (newValue as Action)?.Invoke());

        //public static BindableProperty EnteredStateProperty = BindableProperty.CreateAttached("EnteredState", typeof(Action), typeof(IAnimatable), null, propertyChanged: (bindable, oldValue, newValue) => (newValue as Action)?.Invoke());

        static StateAnimationExtensions()
        {
            AddSupportForType(values => values[0], value => new double[] { value });
            AddSupportForType(values => (float)values[0], value => new double[] { value });
            AddSupportForType(values => new Color((float)values[0], (float)values[1], (float)values[2], (float)values[3]), color => new double[] { color.Red, color.Green, color.Blue, color.Alpha });
            AddSupportForType(values => new Thickness(values[0], values[1], values[2], values[3]), thickness => new double[] { thickness.Left, thickness.Top, thickness.Right, thickness.Bottom });
            AddSupportForType(values => new Rect(values[0], values[1], values[2], values[3]), rectangle => new double[] { rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height });
            AddSupportForType(values => new GridLength(values[0], (GridUnitType)values[1]), value => new double[] { value.Value, (double)value.GridUnitType });
            //AddSupportForType(values => new AnalogBool { Value = values[0], IntermediateValue = ((int)values[1]).ToBool() }, value => new double[] { value.Value, value.IntermediateValue.ToInt() });
        }

        public static void AddSupportForType<T>(Func<double[], T> constructor, Func<T, double[]> deconstructor) => Transition[typeof(T)] = new DoubleExtractor(value => deconstructor((T)(dynamic)value), values => constructor(values));

        public static Animation AnimationToState<T>(this T self, VisualStateIdentifier start = null, VisualStateIdentifier end = null, Easing easing = null, Action<double> callback = null, Action finished = null)
            where T : VisualElement, IAnimatable
        {
            VisualState startingState = start?.GetVisualState(self) ?? new VisualState();
            VisualState endingState = end.GetVisualState(self);

            if (startingState == null || endingState == null)
            {
                System.Diagnostics.Debug.WriteLine("A visual state with the name " + (startingState == null ? startingState.Name : endingState.Name) + " was not found on the passed object");
                return null;
            }

            List<Action<double>> childCallbacks = new List<Action<double>>(endingState.Setters.Count);
            var startSetters = new List<(BindableObject, BindableProperty, object)>();
            var endSetters = new List<(BindableObject, BindableProperty, object)>();

            Animation animation = new Animation(value =>
            {
                if (value == 0)
                {
                    foreach (var setter in startSetters)
                    {
                        setter.Item1.SetValue(setter.Item2, setter.Item3);
                    }
                }

                foreach (Action<double> childCallback in childCallbacks)
                {
                    childCallback(value);
                }
                callback?.Invoke(value);

                if (value == 1)
                {
                    foreach (var setter in endSetters)
                    {
                        setter.Item1.SetValue(setter.Item2, setter.Item3);
                    }
                }
            }, 0, 1, easing, finished);

            int matched = 0;
            for (int i = 0; i < endingState.Setters.Count; i++)
            {
                Setter endSetter = endingState.Setters[i];
                Setter startSetter = null;

                for (int j = matched; j < startingState.Setters.Count; j++)
                {
                    if (endSetter.Property == startingState.Setters[j].Property)
                    {
                        startSetter = startingState.Setters[j];

                        if (j == matched)
                        {
                            matched++;
                        }

                        break;
                    }
                }

                BindableObject target = endSetter.GetTarget() ?? self;
                if ((startSetter == null || target == (startSetter.GetTarget() ?? self)))
                {
                    object startValue = startSetter.GetValue();
                    object endValue = endSetter.GetValue();

                    if (Transition.ContainsKey(endSetter.Property.ReturnType))
                    {
                        animation.Add(0, 1, PropertyAnimation.Create(target, endSetter.Property, endValue, startValue));
                    }
                    else
                    {
                        if (startSetter != null)
                        {
                            startSetters.Add((target, startSetter.Property, startValue));
                        }
                        if (endSetter != null)
                        {
                            (startSetter == null && endValue is BooleanValue ? startSetters : endSetters).Add((target, endSetter.Property, endValue));
                        }
                    }
                }
            }

            return animation;
        }

        private static BindableObject GetTarget(this Setter setter) => (setter?.Value as UniversalSetter.Pair)?.Target;

        private static object GetValue(this Setter setter) => (setter?.Value as UniversalSetter.Pair)?.Value ?? setter?.Value;

        public static VisualState GetVisualStateByName(this VisualElement visualElement, string name)
        {
            foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(visualElement))
            {
                VisualState visualState = group.States.GetVisualStateByName(name);

                if (visualState != null)
                {
                    return visualState;
                }
            }

            return null;
        }

        public static VisualState GetVisualStateByName(this IEnumerable<VisualState> visualStates, string name)
        {
            foreach (VisualState state in visualStates)
            {
                if (state.Name == name)
                {
                    return state;
                }
            }

            return null;
        }
    }

#if false
    public class AnySetter
    {
        public Action<object> Action { get; set; }
        public Type ReturnType { get; set; }
        public object Value { get; set; }
    }

    public class AnySetter<T> : AnySetter
    {
        new public Action<T> Action
        {
            get => value => base.Action(value);
            set => base.Action = temp => value((T)temp);
        }

        public AnySetter()
        {
            ReturnType = typeof(T);
        }
    }

    public class AnyVisualState
    {
        public string Name { get; set; }

        public DualList<Setter, AnySetter> Setters { get; } = new DualList<Setter, AnySetter>();

        public Type TargetType { get; set; }

        public static implicit operator AnyVisualState(VisualState state)
        {
            AnyVisualState result = new AnyVisualState
            {
                Name = state.Name,
                TargetType = state.TargetType
            };

            foreach (Setter setter in state.Setters)
            {
                result.Setters.Add(setter);
            }

            return result;
        }

        public static explicit operator VisualState(AnyVisualState state)
        {
            VisualState result = new VisualState
            {
                Name = state.Name,
                TargetType = state.TargetType
            };

            foreach(object setter in state.Setters)
            {
                if (setter is Setter propertySetter)
                {
                    result.Setters.Add(propertySetter);
                }
            }

            return result;
        }
    }

    public static VisualState GetIntermediateVisualState(double value, VisualState state1, VisualState state2)
        {
            if (state1.Setters.Count > state2.Setters.Count)
            {
                Misc.Swap(ref state1, ref state2);
            }

            VisualState intermediate = new VisualState
            {
                Name = value + " between " + state1 + " and " + state2
            };
            int matched = 0;

            for (int i = 0; i < state1.Setters.Count; i++)
            {
                Setter startSetter = state1.Setters[i];
                DoubleExtractor extractor;
                
                if (!Transition.TryGetValue(startSetter.Property.ReturnType, out extractor))
                {
                    continue;
                }

                for (int j = matched; j < state2.Setters.Count; j++)
                {
                    Setter endSetter = state2.Setters[j];

                    if (startSetter.Property == endSetter.Property)
                    {
                        double[] start = extractor.Item1(startSetter.Value);
                        double[] end = extractor.Item1(endSetter.Value);
                        //Print.Log("\t" + startSetter.Value, endSetter.Value);
                        double[] values = new double[start.Length];
                        for (int k = 0; k < values.Length; k++)
                        {
                            //Print.Log("\t" + start[k], end[k]);
                            values[k] = start[k] + value * (end[k] - start[k]);
                        }

                        intermediate.Setters.Add(new Setter { Property = startSetter.Property, Value = extractor.Item2(values) });
                        //Print.Log("value is ", value, extractor.Item2(values));
                        //list.Add(new Tuple<Constructor, Setter, double[], double[]>(extractor.Item2, startSetter, extractor.Item1(startSetter.Value), extractor.Item1(endSetter.Value)));

                        if (j == matched)
                        {
                            matched++;
                        }

                        break;
                    }
                }
            }

            return intermediate;
        }

        public static void AnimateVisualState<T>(this T self, string name, VisualStateIdentifier end, uint rate = 16, uint length = 250, Easing easing = null, Action<double> callback = null, Action<double, bool> finished = null, Func<bool> repeat = null)
            where T : VisualElement, IAnimatable
        {
            VisualState endingState = end.GetVisualState(self);

            VisualState start = new VisualState
            {
                Name = self.GetHashCode() + "CurrentState",
                TargetType = endingState.TargetType
            };
            
            foreach(Setter setter in endingState.Setters)
            {
                start.Setters.Add(new Setter
                {
                    Property = setter.Property,
                    TargetName = setter.TargetName,
                    Value = self.GetValue(setter.Property)
                });
            }

            AnimateVisualState(self, name, start, end, rate, length, easing, callback, finished, repeat);
        }

        public static void AnimateVisualState<T>(this T self, string name, VisualStateIdentifier start, VisualStateIdentifier end, uint rate = 16, uint length = 250, Easing easing = null, Action<double> callback = null, Action<double, bool> finished = null, Func<bool> repeat = null)
            where T : VisualElement, IAnimatable
        {
            VisualStateGroup group = null;
            VisualState startingState = start.GetVisualState(self);
            VisualState intermediate = null;
            VisualState endingState = end.GetVisualState(self);

            if (startingState == null || endingState == null)
            {
                System.Diagnostics.Debug.WriteLine("A visual state with the name " + (startingState == null ? startingState.Name : endingState.Name) + " was not found on the passed object");
                return;
            }

            IList<VisualStateGroup> visualStateGroups = VisualStateManager.GetVisualStateGroups(self);
            for (int i = 0; i < visualStateGroups.Count; i++)
            {
                VisualStateGroup parentGroup = visualStateGroups[i];

                foreach (VisualState state in parentGroup.States)
                {
                    if (state == startingState || state == endingState)
                    {
                        if (group == null)
                        {
                            group = parentGroup;
                        }
                        else if (group != parentGroup)
                        {
                            System.Diagnostics.Debug.Write("The two visual states to transition between must be in the same VisualStateGroup");
                            return;
                        }
                        else
                        {
                            i = visualStateGroups.Count;
                            break;
                        }
                    }
                }
            }

            if (group == null)
            {
                Print.Log("error");
                return;
            }

            void Callback(double value)
            {
                intermediate = group.States[group.States.IndexOf(intermediate)] = GetIntermediateVisualState(value, startingState, endingState);
                VisualStateManager.GoToState(self, intermediate.Name);
                callback?.Invoke(value);
            }

            group.States.Add(intermediate = GetIntermediateVisualState(0, startingState, endingState));
            VisualStateManager.GoToState(self, startingState.Name);
            self.Animate(name, Callback, 0, 1, rate, length, easing, (final, cancelled) =>
            {
                VisualStateManager.GoToState(self, endingState.Name);
                group.States.Remove(intermediate);
                finished?.Invoke(final, cancelled);
            }, repeat);
        }

    public class VisualStateGroup<T> : List<VisualState<T>>
    {
        public void Add(BindableProperty property, params T[] values) => AddGeneric(property, values);
        public void Add(Action<T> setter, params T[] values) => AddGeneric(setter, values);

        private void AddGeneric(dynamic property, params T[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                StateAt(i).Add(property, values[i]);
            }
        }

        private VisualState<T> StateAt(int index)
        {
            if (index >= Count)
            {
                for (int i = 0; i < index - Count + 1; i++)
                {
                    Add(new VisualState<T>());
                }
            }
            
            return this[index];
        }
    }
    
    public class AnimatedTransition
    {
        public VisualStateGroup<double> States;

        public Animation Animate(int fromState, int toState, BindableObject bindable = null)
        {
            VisualState<double> from = States[fromState];
            VisualState<double> to = States[toState];

            Animation animation = new Animation();

            for (int i = 0; i < Math.Min(from.Count, to.Count); i++)
            {

            }

            return animation;
        }
    }

    public class AnimationState
    {
        public class Tuple : Tuple<BindableProperty, IList<object>>
        {
            public Tuple(BindableProperty property, params object[] values) : base(property, values) { }
        }

        public List<VisualState> States { get; }

        public AnimationState(params VisualState[] states) => States = new List<VisualState>(states);

        public AnimationState(params Tuple[] properties)//, IEnumerable<Tuple<Action<object>, IList<object>>> callbacks)
        {
            /*if (properties.Item2.Count != callbacks.Item2.Count)
            {
                throw new Exception("Properties and callbacks must define the same number of states");
            }*/

            int i = 0;
            foreach(var property in properties)
            {
                VisualState state = new VisualState();

                foreach (object o in property.Item2)
                {
                    //state.Properties.Add(new PropertyState(property.Item1, property.Item2[i++]));
                }

                States.Add(state);
            }
        }

        public void Add(Tuple property) => AddGeneric(property);

        private void AddGeneric<T>(Tuple<T, IList<object>> property)
        {
            for (int i = 0; i < property.Item2.Count; i++)
            {
                States[i].Add((dynamic)property.Item1, property.Item2[i]);
            }
        }
    }
#endif
}
