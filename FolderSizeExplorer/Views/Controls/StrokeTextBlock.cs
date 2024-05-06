using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FolderSizeExplorer.Views.Controls
{
    public class StrokeTextBlock : TextBlock
    {
        private StrokeAdorner _adorner;
        private bool _adorned = false;

        public StrokeTextBlock()
        {
            _adorner = new StrokeAdorner(this);
            this.LayoutUpdated += StrokeTextBlock_LayoutUpdated;
        }

        private void StrokeTextBlock_LayoutUpdated(object sender, EventArgs e)
        {
            if (_adorned) return;
            _adorned = true;
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            adornerLayer.Add(_adorner);
            this.LayoutUpdated -= StrokeTextBlock_LayoutUpdated;
        }

        private static void strokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stb = (StrokeTextBlock)d;
            stb._adorner.Stroke = e.NewValue as Brush;
        }

        private static void strokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stb = (StrokeTextBlock)d;
            stb._adorner.StrokeThickness = DependencyProperty.UnsetValue.Equals(e.NewValue) ? (ushort)0 : (ushort)e.NewValue;
        }

        /// <summary>
        /// Specifies the brush to use for the stroke and optional hightlight of the formatted text.
        /// </summary>
        public Brush Stroke
        {
            get
            {
                return (Brush)GetValue(StrokeProperty);
            }

            set
            {
                SetValue(StrokeProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Stroke dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke",
            typeof(Brush),
            typeof(StrokeTextBlock),
            new FrameworkPropertyMetadata(
                    new SolidColorBrush(Colors.Teal),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(strokeChanged),
                    null
                    )
            );

        /// <summary>
        ///     The stroke thickness of the font.
        /// </summary>
        public ushort StrokeThickness
        {
            get
            {
                return (ushort)GetValue(StrokeThicknessProperty);
            }

            set
            {
                SetValue(StrokeThicknessProperty, value);
            }
        }

        /// <summary>
        /// Identifies the StrokeThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness",
            typeof(ushort),
            typeof(StrokeTextBlock),
            new FrameworkPropertyMetadata(
                    (ushort)0,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(strokeThicknessChanged),
                    null
                    )
            );
    }
}
