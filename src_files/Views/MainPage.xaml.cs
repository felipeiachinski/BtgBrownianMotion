using BrownianSim.Drawables;
using BrownianSim.ViewModels;

namespace BrownianSim.Views;

public partial class MainPage : ContentPage
{
    private readonly BrownianChartDrawable _drawable;

    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        _drawable = new BrownianChartDrawable(vm.GetSnapshot);
        Chart.Drawable = _drawable;

        vm.RequestRedraw += (_, __) => Chart.Invalidate();
        vm.SimulateCommand.Execute(null);
    }
}
