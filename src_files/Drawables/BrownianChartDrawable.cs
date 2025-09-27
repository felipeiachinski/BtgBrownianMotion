using Microsoft.Maui.Graphics;

namespace BrownianSim.Drawables;

public sealed class BrownianChartDrawable : IDrawable
{
    private readonly System.Func<BrownianSim.ViewModels.ChartSnapshot> _snapshotProvider;
    public BrownianChartDrawable(System.Func<BrownianSim.ViewModels.ChartSnapshot> snapshotProvider) => _snapshotProvider = snapshotProvider;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var snap = _snapshotProvider();
        canvas.SaveState();
        canvas.Antialias = true;

        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        float m = 40f;
        var plot = new RectF(dirtyRect.Left + m, dirtyRect.Top + m * 0.5f, dirtyRect.Width - m * 1.5f, dirtyRect.Height - m * 1.5f);

        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 1;
        canvas.DrawLine(plot.Left, plot.Bottom, plot.Right, plot.Bottom);
        canvas.DrawLine(plot.Left, plot.Top, plot.Left, plot.Bottom);

        float min = snap.Min;
        float max = snap.Max;
        if (System.Math.Abs(max - min) < 1e-6f) { max += 1; min -= 1; }

        if (snap.ShowGrid)
        {
            int vTicks = 5;
            for (int i = 0; i <= vTicks; i++)
            {
                float y = plot.Top + i * (plot.Height / vTicks);
                canvas.StrokeColor = Colors.LightGray;
                canvas.DrawLine(plot.Left, y, plot.Right, y);

                float value = max - i * (max - min) / vTicks;
                canvas.StrokeColor = Colors.Black;
                canvas.DrawString(value.ToString("F2"), 6, y - 8, HorizontalAlignment.Left);
            }

            var n = snap.Series.FirstOrDefault()?.Count ?? 0;
            int hTicks = 5;
            for (int i = 0; i <= hTicks; i++)
            {
                float x = plot.Left + i * (plot.Width / hTicks);
                canvas.StrokeColor = Colors.LightGray;
                canvas.DrawLine(x, plot.Top, x, plot.Bottom);

                int day = n > 0 ? (int)System.Math.Round(i * (n - 1) / (double)hTicks) : 0;
                canvas.StrokeColor = Colors.Black;
                canvas.DrawString(day.ToString(), x - 8, plot.Bottom + 4, HorizontalAlignment.Left);
            }
        }

        Color[] palette = new[]
        {
            Colors.Blue, Colors.Red, Colors.Green, Colors.Orange, Colors.Purple,
            Colors.Brown, Colors.Teal, Colors.Crimson, Colors.Gold, Colors.SlateBlue
        };

        int sIdx = 0;
        foreach (var series in snap.Series)
        {
            if (series.Count < 2) { sIdx++; continue; }

            var color = palette[sIdx % palette.Length];
            canvas.StrokeColor = color;
            canvas.StrokeSize = 2;

            Microsoft.Maui.Graphics.PointF? prev = null;
            int n = series.Count;
            for (int i = 0; i < n; i++)
            {
                float x = plot.Left + (i / (float)(n - 1)) * plot.Width;
                float y = plot.Bottom - ((series[i].Y - min) / (max - min)) * plot.Height;
                if (prev != null) canvas.DrawLine(prev.Value.X, prev.Value.Y, x, y);
                prev = new Microsoft.Maui.Graphics.PointF(x, y);
            }
            sIdx++;
        }

        if (snap.ShowLegend && snap.Series.Count > 1)
        {
            float lx = plot.Right - 120;
            float ly = plot.Top + 10;
            for (int i = 0; i < snap.Series.Count; i++)
            {
                canvas.StrokeColor = palette[i % palette.Length];
                canvas.DrawLine(lx, ly + i * 18, lx + 20, ly + i * 18);

                canvas.StrokeColor = Colors.Black;
                canvas.DrawString($"Sim {i + 1}", lx + 26, ly - 8 + i * 18, HorizontalAlignment.Left);
            }
        }

        canvas.RestoreState();
    }
}
