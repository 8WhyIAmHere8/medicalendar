using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using medi1.Pages;               // for ConditionViewModel

namespace medi1.Pages
{
    public partial class AddEntryPage : ContentPage
    {
        public AddEntryPage()
        {
            InitializeComponent();
        }

        private async void OnAddConditionClicked(object sender, EventArgs e)
        {
            string name = NameEntry.Text?.Trim();
            string description = DescriptionEditor.Text?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                await DisplayAlert("Error", "Please enter a name for the condition.", "OK");
                return;
            }

            // Instantiate the same view-model your HomePage is expecting
            var newCondition = new ConditionViewModel
            {
                Name        = name,
                Description = description,
                IsSelected  = false,
                Color       = GetRandomColor()
            };

            // Send it *from this page* so the HomePage subscription matches:
            MessagingCenter.Send<AddEntryPage, ConditionViewModel>(
                this,
                "ConditionAdded",
                newCondition
            );

            await Navigation.PopAsync();
        }

        private Color GetRandomColor()
        {
            var random = new Random();
            return Color.FromRgb(
                random.Next(0, 256),
                random.Next(0, 256),
                random.Next(0, 256)
            );
        }
    }
}
