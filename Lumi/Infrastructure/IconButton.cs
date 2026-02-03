using System;
using System.Windows;
using System.Windows.Media;

namespace Lumi.Infrastructure
{
    public static class IconButton
    {
        // Guard to prevent recursion when we set IconData internally
        private static readonly DependencyProperty IsCenteringProperty =
            DependencyProperty.RegisterAttached(
                "IsCentering",
                typeof(bool),
                typeof(IconButton),
                new PropertyMetadata(false));

        private static bool GetIsCentering(DependencyObject d) => (bool)d.GetValue(IsCenteringProperty);
        private static void SetIsCentering(DependencyObject d, bool v) => d.SetValue(IsCenteringProperty, v);

        // -----------------------------
        // AutoCenter (global switch)
        // -----------------------------
        public static readonly DependencyProperty AutoCenterProperty =
            DependencyProperty.RegisterAttached(
                "AutoCenter",
                typeof(bool),
                typeof(IconButton),
                new PropertyMetadata(true, OnIconLayoutChanged));

        public static void SetAutoCenter(DependencyObject element, bool value)
            => element.SetValue(AutoCenterProperty, value);

        public static bool GetAutoCenter(DependencyObject element)
            => (bool)element.GetValue(AutoCenterProperty);

        // -----------------------------
        // StrokeThickness (used for centering bounds)
        // -----------------------------
        public static readonly DependencyProperty IconStrokeThicknessProperty =
            DependencyProperty.RegisterAttached(
                "IconStrokeThickness",
                typeof(double),
                typeof(IconButton),
                new PropertyMetadata(2.0, OnIconLayoutChanged));

        public static void SetIconStrokeThickness(DependencyObject element, double value)
            => element.SetValue(IconStrokeThicknessProperty, value);

        public static double GetIconStrokeThickness(DependencyObject element)
            => (double)element.GetValue(IconStrokeThicknessProperty);

        // -----------------------------
        // SVG / Geometry
        // -----------------------------
        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.RegisterAttached(
                "IconData",
                typeof(Geometry),
                typeof(IconButton),
                new PropertyMetadata(null, OnIconDataChanged));

        public static void SetIconData(DependencyObject element, Geometry value)
            => element.SetValue(IconDataProperty, value);

        public static Geometry GetIconData(DependencyObject element)
            => (Geometry)element.GetValue(IconDataProperty);

        // -----------------------------
        // Fill Colors (deine bestehenden)
        // -----------------------------
        public static readonly DependencyProperty IconFillProperty =
            DependencyProperty.RegisterAttached(
                "IconFill",
                typeof(Brush),
                typeof(IconButton),
                new PropertyMetadata(Brushes.White));

        public static void SetIconFill(DependencyObject element, Brush value)
            => element.SetValue(IconFillProperty, value);

        public static Brush GetIconFill(DependencyObject element)
            => (Brush)element.GetValue(IconFillProperty);

        public static readonly DependencyProperty IconFillHoverProperty =
            DependencyProperty.RegisterAttached(
                "IconFillHover",
                typeof(Brush),
                typeof(IconButton),
                new PropertyMetadata(Brushes.White));

        public static void SetIconFillHover(DependencyObject element, Brush value)
            => element.SetValue(IconFillHoverProperty, value);

        public static Brush GetIconFillHover(DependencyObject element)
            => (Brush)element.GetValue(IconFillHoverProperty);

        public static readonly DependencyProperty IconFillPressedProperty =
            DependencyProperty.RegisterAttached(
                "IconFillPressed",
                typeof(Brush),
                typeof(IconButton),
                new PropertyMetadata(Brushes.White));

        public static void SetIconFillPressed(DependencyObject element, Brush value)
            => element.SetValue(IconFillPressedProperty, value);

        public static Brush GetIconFillPressed(DependencyObject element)
            => (Brush)element.GetValue(IconFillPressedProperty);

        // -----------------------------
        // Callbacks
        // -----------------------------
        private static void OnIconDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (GetIsCentering(d)) return;
            if (!GetAutoCenter(d)) return;
            if (e.NewValue is not Geometry geo) return;

            var thickness = GetIconStrokeThickness(d);
            var centered = CreateCenteredGeometry(geo, thickness);

            // If nothing to do, stop.
            if (ReferenceEquals(centered, geo)) return;

            try
            {
                SetIsCentering(d, true);
                d.SetValue(IconDataProperty, centered);
            }
            finally
            {
                SetIsCentering(d, false);
            }
        }

        private static void OnIconLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (GetIsCentering(d)) return;
            if (!GetAutoCenter(d)) return;

            var geo = GetIconData(d);
            if (geo == null) return;

            var thickness = GetIconStrokeThickness(d);
            var centered = CreateCenteredGeometry(geo, thickness);

            if (ReferenceEquals(centered, geo)) return;

            try
            {
                SetIsCentering(d, true);
                d.SetValue(IconDataProperty, centered);
            }
            finally
            {
                SetIsCentering(d, false);
            }
        }

        // -----------------------------
        // Centering logic
        // -----------------------------
        private static Geometry CreateCenteredGeometry(Geometry source, double strokeThickness)
        {
            // Compute bounds including stroke (outline icons)
            var pen = new Pen(Brushes.Black, Math.Max(0.0, strokeThickness));
            Rect bounds = source.GetRenderBounds(pen);

            if (bounds.IsEmpty ||
                double.IsNaN(bounds.X) || double.IsNaN(bounds.Y) ||
                double.IsNaN(bounds.Width) || double.IsNaN(bounds.Height))
            {
                return source;
            }

            double cx = bounds.X + (bounds.Width / 2.0);
            double cy = bounds.Y + (bounds.Height / 2.0);

            // Already centered? Then don't create a new Geometry (prevents endless “new instance” loops)
            // Target center in our design box (24x24)
            const double target = 12.0;
            const double eps = 0.001;

            // Already centered to the 24x24 box center? Then do nothing.
            if (Math.Abs(cx - target) < eps && Math.Abs(cy - target) < eps)
                return source;

            // Clone so we never mutate shared StaticResource geometries
            var geo = source.CloneCurrentValue();

            // Move geometry so its center lands at (12,12)
            var translate = new TranslateTransform(target - cx, target - cy);


            if (geo.Transform != null && !geo.Transform.Value.IsIdentity)
            {
                var group = new TransformGroup();
                group.Children.Add(geo.Transform);
                group.Children.Add(translate);
                geo.Transform = group;
            }
            else
            {
                geo.Transform = translate;
            }

            if (geo.CanFreeze) geo.Freeze();
            return geo;
        }
    }
}
