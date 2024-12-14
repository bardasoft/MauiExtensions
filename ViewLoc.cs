namespace Microsoft.Maui.Controls.Compatibility
{
    public class ViewLoc : ViewLoc<Layout<View>>
    {
        public ViewLoc(Layout<View> parent, int index) : base(parent, index) { }
    }

    public class ViewLoc<T> where T : class
    {
        public T Parent { get; private set; }
        public int Index { get; private set; }

        public ViewLoc(T parent, int index)
        {
            Parent = parent;
            Index = index;
        }

        public override bool Equals(object obj)
        {
            ViewLoc<T> location = obj as ViewLoc<T>;
            return location != null && location.Parent == Parent && location.Index == Index;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
