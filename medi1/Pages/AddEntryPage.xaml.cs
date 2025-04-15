using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics; 
using System;
using medi1.Pages; 

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

            // Create a new condition with a random color.
            var newCondition = new Condition 
            {
                Name = name,
                Description = description,
                IsSelected = false,
                Color = GetRandomColor()
            };

            // Send the new condition using MessagingCenter.
            MessagingCenter.Send(newCondition, "ConditionAdded");

            // Navigate back to HomePage.
            await Navigation.PopAsync();
        }

        private Color GetRandomColor()
        {
            Random random = new Random();
            int r = random.Next(0, 256);
            int g = random.Next(0, 256);
            int b = random.Next(0, 256);
            return Color.FromRgb(r, g, b);
        }
    }
}
