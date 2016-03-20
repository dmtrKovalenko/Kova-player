using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kova.NAudioCore.Visualisation
{
    class ReflectionControl : Decorator
    {
        private VisualBrush _reflection;
        private LinearGradientBrush _opacityMask;

        public ReflectionControl()
        {
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;

            _opacityMask = new LinearGradientBrush();
            _opacityMask.StartPoint = new Point(0, 0);
            _opacityMask.EndPoint = new Point(0, 1);
            _opacityMask.GradientStops.Add(new GradientStop(Colors.Black, 0));
            _opacityMask.GradientStops.Add(new GradientStop(Colors.Black, 0.5));
            _opacityMask.GradientStops.Add(new GradientStop(Colors.Transparent, 0.8));
            _opacityMask.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
            _reflection = new VisualBrush();
            _reflection.AlignmentY = AlignmentY.Bottom;
            _reflection.Stretch = Stretch.None;
            _reflection.TileMode = TileMode.None;
            _reflection.Transform = new ScaleTransform(1, -1);
            _reflection.AutoLayoutContent = false;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Control is twice the height of the child control.
            if (Child == null)
            {
                return new Size(0, 0);
            }
            Child.Measure(constraint);
            return new Size(Child.DesiredSize.Width, Child.DesiredSize.Height * 2);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            // Put actual child in the upper half of the control.
            if (Child == null)
            {
                return new Size(0, 0);
            }
            Child.Arrange(new Rect(0, 0, arrangeBounds.Width, arrangeBounds.Height / 2));
            return arrangeBounds;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // Draw non-reflection controls
            base.OnRender(drawingContext);

            double halfHeight = ActualHeight / 2;

            // Create fading opacity mask
            drawingContext.PushOpacityMask(_opacityMask);
            drawingContext.PushOpacity(0.15);

            // Create the reflection mirror transform.
            _reflection.Visual = Child;
            ((ScaleTransform)_reflection.Transform).CenterY = ActualHeight * 3 / 4;
            ((ScaleTransform)_reflection.Transform).CenterX = ActualWidth / 2;

            // Draw the reflection visual with opacity mask applied
            drawingContext.DrawRectangle(
                _reflection, null,
                new Rect(0, halfHeight, ActualWidth, halfHeight));

            // Remove opacity masks for next drawing
            drawingContext.Pop();
            drawingContext.Pop();
        }
    }
}
