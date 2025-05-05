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

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.RecentHealthEvents))
                {
                    Debug.WriteLine($"[Chart Debug] RecentHealthEvents updated. Count: {_viewModel.RecentHealthEvents.Count}");
                    SimpleChart.InvalidateSurface();
                }
            };

            // Load conditions when the page is loaded
            Loaded += async (s, e) =>
            {
                await _viewModel.LoadConditionsCommand.ExecuteAsync(null);
                SimpleChart.InvalidateSurface(); // Ensure chart is refreshed after loading
            };
        }

        private async void AddNewEntry(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new AddEntryPage());
        }
        


        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var vm = BindingContext as ViewModels.ConditionsViewModel;
            var recentEvents = vm?.RecentHealthEvents;

            Debug.WriteLine($"[Chart Debug] PaintSurface triggered. Recent events count: {recentEvents?.Count ?? 0}");

            if (recentEvents == null || recentEvents.Count == 0)
                return;

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            float canvasWidth = e.Info.Width;
            float canvasHeight = e.Info.Height;

            float startX = 60;
            float startY = canvasHeight - 80;
            float barWidth = 40;
            float space = 30;

            var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 18,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            // Draw axis
            canvas.DrawLine(startX - 20, startY, canvasWidth - 20, startY, new SKPaint
            {
                Color = SKColors.Gray,
                StrokeWidth = 2
            });

            for (int i = 0; i < recentEvents.Count; i++)
            {
                var ev = recentEvents[i];

                float severityHeight = ev.Severity * 10;
                float barHeight = Math.Max(severityHeight, 10);

                float left = startX + i * (barWidth + space);
                float top = startY - barHeight;
                float centerX = left + barWidth / 2;

                paint.Color = ev.Severity switch
                {
                    <= 3 => SKColors.Green,
                    <= 6 => SKColors.Yellow,
                    <= 9 => SKColors.Orange,
                    10 => SKColors.Red,
                    _ => SKColors.Gray
                };

                canvas.DrawRect(left, top, barWidth, barHeight, paint);
                canvas.DrawText(Truncate(ev.Title, 8), centerX, startY + 20, textPaint);
                canvas.DrawText(ev.StartDate.ToString("MM/dd"), centerX, startY + 40, textPaint);
            }
        }

        private string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }
    }
}
