using Microsoft.Maui.Controls;

namespace medi1.Pages.AddEntryPageFolder
{
    public partial class AddEntryPage : ContentPage
    {
        public AddEntryPage()
        {
            InitializeComponent();
            BindingContext = new AddEntryViewModel();
        }
    }
}



