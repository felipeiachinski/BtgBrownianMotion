using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using BrownianSim.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace BrownianSim.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private double initialPrice = 100.0;
    [ObservableProperty] private double volatility    = 20.0; // % ao dia (ex.: 20 => 20%)
    [ObservableProperty] private double meanReturn    = 1.0;  // % ao dia (ex.: 1 => 1%)
    [ObservableProperty] private int    days          = 252;  // ~um ano de pregão
    [ObservableProperty] private int    simulations   = 1;    // 1..10

    [ObservableProperty] private bool showGrid   = true;
    [ObservableProperty] private bool showLegend = true;

    public event EventHandler? RequestRedraw;

    public ObservableCollection<IReadOnlyList<PointF>> Series { get; } = new();

    [RelayCommand]
    private void Simulate()
    {
        Series.Clear();

        for (int s = 0; s < Simulations; s++)
        {
            var sigma = Volatility / 100.0; var mu = MeanReturn / 100.0;
            var arr = Brownian.GenerateBrownianMotion(sigma, mu, InitialPrice, Days);
            var pts = new List<PointF>(arr.Length);
            for (int i = 0; i < arr.Length; i++)
                pts.Add(new PointF(i, (float)arr[i]));
            Series.Add(pts);
        }

        OnPropertyChanged(nameof(Min));
        OnPropertyChanged(nameof(Max));
        OnPropertyChanged(nameof(Count));
        RequestRedraw?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Clear()
    {
        Series.Clear();
        OnPropertyChanged(nameof(Min));
        OnPropertyChanged(nameof(Max));
        OnPropertyChanged(nameof(Count));
        RequestRedraw?.Invoke(this, EventArgs.Empty);
    }

    public float Min  => Series.SelectMany(s => s).Select(p => p.Y).DefaultIfEmpty((float)InitialPrice).Min();
    public float Max  => Series.SelectMany(s => s).Select(p => p.Y).DefaultIfEmpty((float)InitialPrice).Max();
    public int   Count => Series.FirstOrDefault()?.Count ?? 0;

    public ChartSnapshot GetSnapshot() => new()
    {
        Series     = Series.ToList(),
        Min        = Min,
        Max        = Max,
        ShowGrid   = ShowGrid,
        ShowLegend = ShowLegend
    };
}

public sealed class ChartSnapshot
{
    public List<IReadOnlyList<PointF>> Series { get; init; } = new();
    public float Min { get; init; }
    public float Max { get; init; }
    public bool ShowGrid { get; init; }
    public bool ShowLegend { get; init; }
}
