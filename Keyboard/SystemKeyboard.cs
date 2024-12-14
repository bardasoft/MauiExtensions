#if false
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
    /*public static class SystemKeyboard
    {
        public static ISoftKeyboard Instance => RawInstance;
        //public static bool IsShowing => KeyboardManager.Current == Instance;

        private static readonly KeyboardEntry RawInstance = KeyboardEntry.Create();

        public static void Setup(AbsoluteLayout layout)
        {
            layout.Children.Add(RawInstance, new Point(-1000, -1000));
        }

    }*/

    /*public class Key : BindableObject
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Key), null);

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public object Parameter { get; set; }
    }*/

    /*public class CommandExtension : IMarkupExtension<Key>
    {
        public object Command { get; set; }
        public object Parameter { get; set; }

        public Key ProvideValue(IServiceProvider serviceProvider)
        {
            var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();

            BindableProperty property = (valueProvider.TargetObject as Setter)?.Property ?? valueProvider.TargetProperty as BindableProperty;

            Print.Log(Forward(Command, serviceProvider));
            ;
            return new Key
            {
                Command = Forward(Command, serviceProvider) as ICommand,
                Parameter = Forward(Parameter, serviceProvider)
            };
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);

        private object Forward(object value, IServiceProvider serviceProvider) => value is IMarkupExtension markup ? markup.ProvideValue(serviceProvider) : value;
    }*/

    public class Key : Button
    {
        public enum Keycode
        {
            Backspace,
            LeftArrow,
            RightArrow,
            UpArrow,
            DownArrow,
            Home,
            End,
        }

        public Keycode Code { get; set; }

        new public object CommandParameter => (object)Code ?? Text;
    }

    public class SysKeyboardViewModel
    {
        public static SysKeyboardViewModel Instance { get; private set; }

        public static event KeystrokeEventHandler Typed;

        public ICommand AlphaNumericCommand { get; private set; } = new KeyCommand();
        public ICommand BackspaceCommand { get; private set; } = new KeyCommand();
        public ICommand LeftArrowCommand { get; private set; } = new KeyCommand();
        public ICommand RightArrowCommand { get; private set; } = new KeyCommand();
        public ICommand UpArrowCommand { get; private set; } = new KeyCommand();
        public ICommand DownArrowCommand { get; private set; } = new KeyCommand();

        private static void OnTyped(object key)
        {
            Typed?.Invoke(key.ToString());
        }

        public class KeyCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            private object Keycode;

            public KeyCommand(object keycode = null)
            {
                Keycode = keycode;
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter) => OnTyped(Keycode ?? parameter);
        }
    }

    public class KeyboardEntry : Entry, ISoftKeyboard
    {
        public event KeystrokeEventHandler Typed;
        public event EventHandler OnscreenSizeChanged;
        public Size Size { get; private set; }

        public bool Showing = false;

        public static KeyboardEntry Instance;

        public ICommand AlphaNumericCommand { get; set; }

        public KeyboardEntry()
        {
            //Keyboard = Keyboard.Plain;
            
            TextChanged += (sender, e) =>
            {
                if (Text == "  ")
                {
                    return;
                }

                string text = Text.Length < 2 ? KeyboardManager.BACKSPACE.ToString() : Text.Trim();
                AlphaNumericCommand?.Execute(text);
                Typed?.Invoke(text);
                Reset();
            };
        }

        public static KeyboardEntry Create() => Instance ?? (Instance = new KeyboardEntry());

        public void Disable(bool animated = false)
        {
            if (!Showing)
            {
                return;
            }

            Showing = false;
            Unfocus();
        }

        public void Enable(bool animated = false)
        {
            if (Showing)
            {
                return;
            }

            Showing = true;
            Focus();
            Reset();
        }

        public void DismissedBySystem()
        {
            if (!Showing)
            {
                return;
            }

            SoftKeyboardManager.OnDismissed();
            //SoftKeyboardManager.NextKeyboard();
        }

        public void OnOnscreenSizeChanged(Size size)
        {
            Size = size;
            OnscreenSizeChanged?.Invoke(this, new EventArgs());
        }

        protected override void OnParentSet()
        {
            if (this.Parent<Page>() is Page oldParent)
            {
                oldParent.Appearing -= Unhide;
            }

            base.OnParentSet();

            if (this.Parent<Page>() is Page newParent)
            {
                newParent.Appearing += Unhide;
            }
        }

        private void Reset()
        {
            Text = "  ";
            CursorPosition = 1;
        }

        private void Unhide(object sender, EventArgs e)
        {
            if (Showing)
            {
                Focus();
            }
        }
    }
}
#endif