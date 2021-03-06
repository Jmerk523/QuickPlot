﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/* Most functions in this module are user-facing.
 * Users are encouraged only to work with Figure-level and Plot-level functions.
 * These functions hide complexity and make refactoring easier.
 * This file will be HUGE, but it is the primary interface for all plotting functions.
 */

namespace QuickPlot
{
    public class Plot
    {
        // TODO: make all properties private

        readonly PlotSettings.Label title = new PlotSettings.Label { fontSize = 16, weight = SKFontStyleWeight.Bold };
        readonly PlotSettings.Label yLabel = new PlotSettings.Label { fontSize = 16, weight = SKFontStyleWeight.SemiBold };
        readonly PlotSettings.Label xLabel = new PlotSettings.Label { fontSize = 16, weight = SKFontStyleWeight.SemiBold };
        readonly PlotSettings.Label y2Label = new PlotSettings.Label { fontSize = 16, weight = SKFontStyleWeight.SemiBold };

        public readonly PlotSettings.Axes axes = new PlotSettings.Axes();
        public readonly PlotSettings.Axes axes2 = new PlotSettings.Axes();

        public PlotSettings.TickCollection yTicks = new PlotSettings.TickCollection(PlotSettings.Side.left);
        public PlotSettings.TickCollection y2Ticks = new PlotSettings.TickCollection(PlotSettings.Side.right);
        public PlotSettings.TickCollection xTicks = new PlotSettings.TickCollection(PlotSettings.Side.bottom);

        public readonly PlotSettings.Layout layout = new PlotSettings.Layout();
        public readonly PlotSettings.SubplotPosition subplotPosition = new PlotSettings.SubplotPosition(1, 1, 1);

        private readonly List<Plottables.Plottable> plottables = new List<Plottables.Plottable>();

        /// <summary>
        /// A data-containing plot (housed inside a Figure)
        /// </summary>
        public Plot()
        {
            axes2.y.display = false;
            AutoAxis();
        }

        #region add or remove plottables

        /// <summary>
        /// Plot data from arrays of matched X/Y points
        /// </summary>
        public void Scatter(
            double[] xs,
            double[] ys,
            string label = null,
            SKColor? color = null,
            bool secondY = false
            )
        {
            var style = new Style(
                label: label,
                color: color,
                plotNumber: plottables.Count,
                secondY: secondY
                );

            var scatterPlot = new Plottables.Scatter(xs, ys, style);
            plottables.Add(scatterPlot);

            if (style.secondY) axes2.y.display = true;

            AutoAxis();
        }

        /// <summary>
        /// Remove all plottables
        /// </summary>
        public void Clear()
        {
            plottables.Clear();
        }

        #endregion

        #region axis management

        private List<Plottables.Plottable> GetPlottablesByAxis(bool secondaryAxis = false)
        {
            List<Plottables.Plottable> plist = new List<Plottables.Plottable>();
            foreach (Plottables.Plottable plottable in plottables)
                if (plottable.style.secondY == secondaryAxis)
                    plist.Add(plottable);
            return plist;
        }

        /// <summary>
        /// Automatically set axis limits to fit the data (with a margin of padding)
        /// </summary>
        public void AutoAxis(
            double marginX = .05, double marginY = .1, 
            bool primaryY = true, bool secondaryY = true
            )
        {
            if (primaryY)
            {
                var primaryPlottables = GetPlottablesByAxis(false);
                if (primaryPlottables.Count > 0)
                {
                    axes.Set(primaryPlottables[0].GetDataArea());
                    for (int i = 1; i < primaryPlottables.Count; i++)
                        axes.Expand(primaryPlottables[i].GetDataArea());
                }
                else
                {
                    axes.Set(-1, 1, -1, 1);
                }
                axes.Zoom(1 - marginX, 1 - marginY);
            }

            if (secondaryY)
            {
                var secondaryPlottables = GetPlottablesByAxis(true);
                if (secondaryPlottables.Count > 0)
                {
                    axes2.Set(secondaryPlottables[0].GetDataArea());
                    for (int i = 1; i < secondaryPlottables.Count; i++)
                        axes2.Expand(secondaryPlottables[i].GetDataArea());
                }
                else
                {
                    axes2.Set(null, null, -1, 1);
                }
                axes2.Zoom(1 - marginX, 1 - marginY);
                axes2.x = axes.x;
            }
        }

        /// <summary>
        /// Make this plot's horizontal axis match that of an existing plot (or null to reset)
        /// </summary>
        public void ShareX(Plot source)
        {
            axes.x = (source is null) ? new PlotSettings.Axis(axes.x.low, axes.x.high) : source.axes.x;
        }

        /// <summary>
        /// Make this plot's vertical axis match that of an existing plot (or null to reset)
        /// </summary>
        public void ShareY(Plot source)
        {
            axes.y = (source is null) ? new PlotSettings.Axis(axes.y.low, axes.y.high) : source.axes.y;
        }

        /// <summary>
        /// Set text and style for the vertical axis label
        /// </summary>
        public void YLabel(
            string text = null, 
            float? fontSize = null, 
            string fontName = null, 
            SKColor? color = null, 
            bool secondY = false
            )
        {
            var lbl = (secondY) ? y2Label : yLabel;
            var tck = (secondY) ? y2Ticks : yTicks;
            lbl.text = text;
            if (fontSize != null) lbl.fontSize = (float)fontSize;
            if (fontName != null) lbl.fontName = fontName;
            if (color != null)
            {
                lbl.fontColor = (SKColor)color;
                if (secondY)
                    y2Ticks.yTickColor = (SKColor)color;
                else
                    yTicks.yTickColor = (SKColor)color;
            }
        }

        public void YTicks(
            SKColor? color = null,
            bool secondY = false
            )
        {
            var ax = (secondY) ? axes2.y : axes.y;
            //if (color!=null)

        }

        /// <summary>
        /// Set text and style for the horizontal axis label
        /// </summary>
        public void XLabel(
            string text = null, 
            float? fontSize = null, 
            string fontName = null,
            SKColor? color = null
            )
        {
            xLabel.text = text;
            if (fontSize != null) xLabel.fontSize = (float)fontSize;
            if (fontName != null) xLabel.fontName = fontName;
            if (color != null) xLabel.fontColor = (SKColor)color;
        }

        /// <summary>
        /// Set text and style for the plot title
        /// </summary>
        public void Title(
            string text = null, 
            float? fontSize = null, 
            string fontName = null,
            SKColor? color = null
            )
        {
            title.text = text;
            if (fontSize != null) title.fontSize = (float)fontSize;
            if (fontName != null) title.fontName = fontName;
            if (color != null) title.fontColor = (SKColor)color;
        }

        #endregion

        #region rendering

        /// <summary>
        /// Draw the plot on the given SKCanvas
        /// </summary>
        public void Render(SKCanvas canvas, SKRect plotRect)
        {
            // this is a chicken-and-egg problem
            UpdateLayout(plotRect); // guess dimensions
            UpdateTicks(); // guess tick density
            UpdateLayout(plotRect); // refine layout based on new ticks
            UpdateTicks(); // refine ticks based on new layout

            // now that the layout is optimized, draw everything
            DrawBackground(canvas);
            DrawTicks(canvas);
            DrawPlottables(canvas);
            DrawLabels(canvas);
            DrawFrame(canvas);
        }

        /// <summary>
        /// Adjust the ticks to fit the current layout
        /// </summary>
        private void UpdateTicks()
        {
            axes.SetDataRect(layout.dataRect);
            axes2.SetDataRect(layout.dataRect);

            yTicks.Generate(axes.y.low, axes.y.high, layout.dataRect);
            y2Ticks.Generate(axes2.y.low, axes2.y.high, layout.dataRect);
            xTicks.Generate(axes.x.low, axes.x.high, layout.dataRect);

            if (yTicks.biggestTickLabelSize.Width > layout.yScaleWidth)
                layout.yScaleWidth = yTicks.biggestTickLabelSize.Width;
        }

        /// <summary>
        /// Adjust the layout to fit the size of all the labels
        /// </summary>
        private void UpdateLayout(SKRect plotRect)
        {
            // TODO: currently there is no way to manually define these values. They are always auto-detected.
            float textPadPx = 3;

            layout.titleHeight = (title.text is null) ? 0 : title.fontSize + textPadPx;
            layout.yLabelWidth = (yLabel.text is null) ? 0 : yLabel.fontSize + textPadPx;
            layout.y2LabelWidth = (y2Label.text is null) ? 0 : y2Label.fontSize + textPadPx;
            layout.xLabelHeight = (xLabel.text is null) ? 0 : xLabel.fontSize + textPadPx;

            layout.yScaleWidth = (!axes.y.display) ? 0 : yTicks.biggestTickLabelSize.Width + textPadPx;
            layout.y2ScaleWidth = (!axes2.y.display) ? 0 : y2Ticks.biggestTickLabelSize.Width + textPadPx;
            layout.xScaleHeight = (!axes.x.display) ? 0 : xTicks.biggestTickLabelSize.Height + textPadPx;

            layout.Update(plotRect);
        }

        private void DrawBackground(SKCanvas canvas)
        {
            var fillPaint = new SKPaint();
            fillPaint.Color = SKColor.Parse("#FFFFFF");
            //fillPaint.Color = Tools.RandomColor(); // useful for assessing when plots are redrawn
            canvas.DrawRect(layout.dataRect, fillPaint);
        }

        private void DrawPlottables(SKCanvas canvas)
        {
            canvas.Save();
            canvas.ClipRect(axes.GetDataRect());
            foreach (var primaryPlottable in GetPlottablesByAxis(false))
                primaryPlottable.Render(canvas, axes);
            foreach (var secondaryPlottable in GetPlottablesByAxis(true))
                secondaryPlottable.Render(canvas, axes2);
            canvas.Restore();
        }

        private void DrawTicks(SKCanvas canvas)
        {
            if (axes.y.display)
                yTicks.Render(canvas, axes);
            if (axes2.y.display)
                y2Ticks.Render(canvas, axes2);
            if (axes.x.display)
                xTicks.Render(canvas, axes);
        }

        private void DrawLabels(SKCanvas canvas)
        {
            int pathExtend = 500;

            if (title.text != null)
            {
                SKPath titlePath = new SKPath();
                SKPaint titlePaint = title.MakePaint();
                float yPixel = layout.titleRect.CenterY() + layout.titleRect.Height / 5;
                titlePath.MoveTo(layout.titleRect.Left - pathExtend, yPixel);
                titlePath.LineTo(layout.titleRect.Right + pathExtend, yPixel);
                canvas.DrawTextOnPath(title.text, titlePath, 0, 0, titlePaint);
            }

            if (yLabel.text != null)
            {
                SKPath yLabelPath = new SKPath();
                SKPaint yLabelPaint = yLabel.MakePaint();
                float xPixel = layout.yLabelRect.Left + yLabelPaint.TextSize - yLabelPaint.TextSize / 5;
                yLabelPath.MoveTo(xPixel, layout.yLabelRect.Bottom + pathExtend);
                yLabelPath.LineTo(xPixel, layout.yLabelRect.Top - pathExtend);
                canvas.DrawTextOnPath(yLabel.text, yLabelPath, 0, 0, yLabelPaint);
            }

            if (y2Label.text != null)
            {
                SKPath y2LabelPath = new SKPath();
                SKPaint y2LabelPaint = y2Label.MakePaint();
                float xPixel = layout.y2LabelRect.Right - y2LabelPaint.TextSize + y2LabelPaint.TextSize / 5;
                y2LabelPath.MoveTo(xPixel, layout.y2LabelRect.Top - pathExtend);
                y2LabelPath.LineTo(xPixel, layout.y2LabelRect.Bottom + pathExtend);
                canvas.DrawTextOnPath(y2Label.text, y2LabelPath, 0, 0, y2LabelPaint);
            }

            if (xLabel.text != null)
            {
                SKPath xLabelPath = new SKPath();
                SKPaint xLabelPaint = xLabel.MakePaint();
                float yPixel = layout.xLabelRect.Bottom;
                xLabelPath.MoveTo(layout.xLabelRect.Left - pathExtend, yPixel);
                xLabelPath.LineTo(layout.xLabelRect.Right + pathExtend, yPixel);
                canvas.DrawTextOnPath(xLabel.text, xLabelPath, 0, 0, xLabelPaint);
            }
        }

        private void DrawFrame(SKCanvas canvas)
        {
            using (var framePaint = new SKPaint())
            {
                framePaint.Color = title.fontColor;
                canvas.DrawLine(layout.dataRect.Left, layout.dataRect.Top, layout.dataRect.Right, layout.dataRect.Top, framePaint);
                framePaint.Color = yTicks.yTickColor;
                canvas.DrawLine(layout.dataRect.Left, layout.dataRect.Bottom, layout.dataRect.Left, layout.dataRect.Top, framePaint);
                framePaint.Color = y2Ticks.yTickColor;
                canvas.DrawLine(layout.dataRect.Right, layout.dataRect.Bottom, layout.dataRect.Right, layout.dataRect.Top, framePaint);
                framePaint.Color = xTicks.xTickColor;
                canvas.DrawLine(layout.dataRect.Left, layout.dataRect.Bottom, layout.dataRect.Right, layout.dataRect.Bottom, framePaint);
            }
        }

        #endregion
    }
}
