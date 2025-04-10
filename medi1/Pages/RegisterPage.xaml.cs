using Microsoft.Maui.Controls;
using medi1.Data; // Import database context
using medi1.Data.Models; // Import Condition model
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; // Allows using ObservableCollection
using System.Threading.Tasks; // Allows using async/await
using System.Diagnostics; // Allows using Debug.WriteLine for logging

namespace medi1.Pages
{
    public partial class RegisterPage : ContentPage
    {
        //  private readonly MedicalDbContext _dbContext = new MedicalDbContext("Users"); // Create an instance of the database context

        // public ObservableCollection<Data.Models.User> Users { get; set; } = new ObservableCollection<Data.Models.User>();
        public RegisterPage()
        {
            InitializeComponent();
        }


            private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}