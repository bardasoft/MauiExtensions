namespace Microsoft.Maui.Controls
{
    public class MaxSizeLabel : Label
    {
        protected override Size MeasureOverride(double widthConstraint, double heightConstraint) => base.MeasureOverride(MaxLines > 1 ? 0 : widthConstraint, heightConstraint);
    }
}
