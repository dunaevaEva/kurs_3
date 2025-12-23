using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using AvApp1.Model;

namespace AvApp1.Controls
{
    public class ChartView : Control
    {
        public static readonly StyledProperty<IEnumerable<ChartItem>> ItemsProperty =
            AvaloniaProperty.Register<ChartView, IEnumerable<ChartItem>>(nameof(Items));

        public static readonly StyledProperty<string> ChartTypeProperty =
            AvaloniaProperty.Register<ChartView, string>(nameof(ChartType), defaultValue: "Bar");

        public static readonly StyledProperty<string> ChartTitleProperty =
            AvaloniaProperty.Register<ChartView, string>(nameof(ChartTitle), defaultValue: "");

        public static readonly StyledProperty<string> TitleAlignmentProperty =
            AvaloniaProperty.Register<ChartView, string>(nameof(TitleAlignment), defaultValue: "Center");

        static ChartView()
        {
            AffectsRender<ChartView>(ItemsProperty, ChartTypeProperty, ChartTitleProperty, TitleAlignmentProperty);
        }
        
        public IEnumerable<ChartItem> Items
        {
            get => GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }
        public string ChartType
        {
            get => GetValue(ChartTypeProperty);
            set => SetValue(ChartTypeProperty, value);
        }
        public string ChartTitle
        {
            get => GetValue(ChartTitleProperty);
            set => SetValue(ChartTitleProperty, value);
        }
        public string TitleAlignment
        {
            get => GetValue(TitleAlignmentProperty);
            set => SetValue(TitleAlignmentProperty, value);
        }
        
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ItemsProperty)
            {
                 var oldList = change.OldValue as INotifyCollectionChanged;
                 var newList = change.NewValue as INotifyCollectionChanged;
                 if (oldList != null) {
                     oldList.CollectionChanged -= OnCollectionChanged;
                     if (change.OldValue is IEnumerable<ChartItem> oldItems)
                         foreach (var item in oldItems) item.PropertyChanged -= OnItemPropertyChanged;
                 }
                 if (newList != null) {
                     newList.CollectionChanged += OnCollectionChanged;
                     if (change.NewValue is IEnumerable<ChartItem> newItems)
                         foreach (var item in newItems) item.PropertyChanged += OnItemPropertyChanged;
                 }
                 InvalidateVisual();
            }
        }
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) 
        {
             if (e.NewItems != null) foreach (ChartItem item in e.NewItems) item.PropertyChanged += OnItemPropertyChanged;
             if (e.OldItems != null) foreach (ChartItem item in e.OldItems) item.PropertyChanged -= OnItemPropertyChanged;
             InvalidateVisual();
        }
        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) => InvalidateVisual();
        
        public override void Render(DrawingContext context)
        {
            base.Render(context);
            var items = Items?.ToList();
            if (items == null || !items.Any()) return;

            double w = Bounds.Width;
            double h = Bounds.Height;
            if (w <= 0 || h <= 0) return;
            
            context.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));
            
            double titleHeight = DrawTitle(context, w);
            
            using (context.PushTransform(Matrix.CreateTranslation(0, titleHeight)))
            {
          
                double availableH = h - titleHeight;

                if (ChartType == "Pie")
                    DrawPieChart(context, items, w, availableH);
                else
                    DrawBarChart(context, items, w, availableH);
            }
        }
        
        private double DrawTitle(DrawingContext context, double width)
        {
            if (string.IsNullOrEmpty(ChartTitle)) return 10; 

            var typeface = new Typeface(Typeface.Default.FontFamily, FontStyle.Normal, FontWeight.Bold);
            var ft = new FormattedText(
                ChartTitle, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                typeface, 24, Brushes.Black); 

            double x = 10;
            if (TitleAlignment == "Center") x = (width - ft.Width) / 2;
            else if (TitleAlignment == "Right") x = width - ft.Width - 10;

            context.DrawText(ft, new Point(x, 10)); 

            return ft.Height + 20; 
        }

        private void DrawBarChart(DrawingContext context, List<ChartItem> items, double w, double h)
        {
            double maxVal = items.Max(x => x.Value);
            if (maxVal == 0) maxVal = 1;

            double topMargin = 30;    
            double bottomMargin = 30; 
            double sidePadding = 10;
            double availableHeight = h - topMargin - bottomMargin;

            if (availableHeight <= 0) return;

            double barWidth = (w - sidePadding * 2) / items.Count;
            double gap = barWidth * 0.2;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                double barHeight = (item.Value / maxVal) * availableHeight;
                var brush = ParseBrush(item.Color);
                
                double x = sidePadding + i * barWidth + gap / 2;
                double y = (h - bottomMargin) - barHeight;

                var rect = new Rect(x, y, Math.Max(1, barWidth - gap), Math.Max(0, barHeight));
                context.DrawRectangle(brush, null, rect);

                DrawText(context, item.Value.ToString(), rect.Center.X, rect.Top - 15);
                DrawText(context, item.Label, rect.Center.X, h - bottomMargin + 5, 11);
            }
        }

        private void DrawPieChart(DrawingContext context, List<ChartItem> items, double w, double h)
        {
            double total = items.Sum(x => x.Value);
            if (total == 0) return;

            double radius = (Math.Min(w, h) / 2) * 0.85; 
            Point center = new Point(w / 2, h / 2);
            if (radius <= 0) return;

            double startAngle = -90;

            foreach (var item in items)
            {
                double sweepAngle = (item.Value / total) * 360;
                if (sweepAngle < 0.1) continue;

                var brush = ParseBrush(item.Color);
                var geometry = CreatePieSegment(center, radius, startAngle, sweepAngle);
                context.DrawGeometry(brush, new Pen(Brushes.White, 2), geometry);

                if (sweepAngle > 15)
                {
                    double midAngleRad = (startAngle + sweepAngle / 2) * (Math.PI / 180);
                    double textRadius = radius * 0.65;
                    double tx = center.X + textRadius * Math.Cos(midAngleRad);
                    double ty = center.Y + textRadius * Math.Sin(midAngleRad);
                    DrawText(context, item.Value.ToString(), tx, ty - 5, 10);
                }
                startAngle += sweepAngle;
            }
        }
        
        private StreamGeometry CreatePieSegment(Point center, double radius, double startAngle, double sweepAngle)
        {
            var geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                double startRad = startAngle * Math.PI / 180;
                double endRad = (startAngle + sweepAngle) * Math.PI / 180;
                Point p1 = new Point(center.X + radius * Math.Cos(startRad), center.Y + radius * Math.Sin(startRad));
                Point p2 = new Point(center.X + radius * Math.Cos(endRad), center.Y + radius * Math.Sin(endRad));
                ctx.BeginFigure(center, true);
                ctx.LineTo(p1);
                bool isLargeArc = sweepAngle > 180;
                ctx.ArcTo(p2, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise);
                ctx.EndFigure(true);
            }
            return geometry;
        }

        private void DrawText(DrawingContext context, string text, double x, double y, double size = 12)
        {
            if (string.IsNullOrEmpty(text)) return;
            var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, size, Brushes.Black);
            context.DrawText(ft, new Point(x - ft.Width / 2, y));
        }

        private IBrush ParseBrush(string colorCode)
        {
            if (string.IsNullOrEmpty(colorCode)) return Brushes.SteelBlue;
            try { return Brush.Parse(colorCode); } catch { return Brushes.Gray; }
        }
    }
}