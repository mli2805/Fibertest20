using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;

namespace Iit.Fibertest.Client
{
    /// <summary>
    /// the custom map of GMapControl 
    /// </summary>
    [Localizable(false)]
    public class Map : GMapControl
    {
        public long ElapsedMilliseconds;

#if DEBUG
        DateTime _start;
        DateTime _end;
        int _delta;

        private int _counter;
        readonly Typeface _tf = new Typeface("GenericSansSerif");
        readonly FlowDirection fd = new FlowDirection();

        /// <summary>
        /// any custom drawing here
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            _start = DateTime.Now;

            base.OnRender(drawingContext);

            _end = DateTime.Now;
            _delta = (int)(_end - _start).TotalMilliseconds;

            var text = new FormattedText(string.Format(CultureInfo.InvariantCulture, "{0:0.0}", Zoom) + "z, " + MapProvider + ", refresh: " + _counter++ + ", load: " + ElapsedMilliseconds + "ms, render: " + _delta + "ms", CultureInfo.InvariantCulture, fd, _tf, 20, Brushes.Blue);
            drawingContext.DrawText(text, new Point(text.Height, text.Height));
            //         text = null;

        }
#endif

    }
}
