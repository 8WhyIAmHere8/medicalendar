using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace medi1.Pages.ConditionsPage

{
    public partial class ConditionsPage : ContentPage
    {
        private readonly ViewModels.ConditionsViewModel _viewModel;

         public ConditionsPage()
    {
        InitializeComponent();
        _viewModel = new ViewModels.ConditionsViewModel();
        BindingContext = _viewModel;

        // Pass ChartCanvas reference
        _viewModel.AttachCanvas(ChartCanvas);

        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.RecentHealthEvents) || e.PropertyName == nameof(_viewModel.SelectedCondition))
            {
                Debug.WriteLine($"[Chart Debug] Property changed: {e.PropertyName}");
                ChartCanvas.InvalidateSurface();
            }
        };

        Loaded += async (s, e) =>
        {
            await _viewModel.LoadConditionsCommand.ExecuteAsync(null);
            ChartCanvas.InvalidateSurface();
        };
    }
        

        private async void AddNewEntry(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new AddEntryPage());
            ChartCanvas.InvalidateSurface();
        }

        
private void OnZoomInClicked(object sender, EventArgs e)
{
    if (BindingContext is ViewModels.ConditionsViewModel vm)
    {
        vm.ZoomIn();
        ChartCanvas.InvalidateSurface(); // Force chart redraw
    }
}

private void OnZoomOutClicked(object sender, EventArgs e)
{
    if (BindingContext is ViewModels.ConditionsViewModel vm)
    {
        vm.ZoomOut();
        ChartCanvas.InvalidateSurface(); // Force chart redraw
    }
}

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            _viewModel.OnCanvasViewPaintSurface(sender, e);
        }
    }
}
