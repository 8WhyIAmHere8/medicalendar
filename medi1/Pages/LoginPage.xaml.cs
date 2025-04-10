using medi1.Data; // Import database context
using medi1.Data.Models; // Import Condition model
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel; // Allows using ObservableCollection
using System.Threading.Tasks; // Allows using async/await
using System.Diagnostics;
using System.Linq.Expressions; // Allows using Debug.WriteLine for logging


namespace medi1.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly MedicalDbContext _dbContext = new MedicalDbContext("Users");

        public ObservableCollection<Data.Models.User> Users { get; set; } = new ObservableCollection<Data.Models.User>();

        private Data.Models.User _selectedUser;
        public Data.Models.User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged(nameof(SelectedUser));
                }
            }
        }
        public LoginPage()
        {
            InitializeComponent();
 

            TestDatabaseConnection(_dbContext);
            LoadUsers();
        }

        private async Task<bool> TestDatabaseConnection(MedicalDbContext dbContext)
        {
            try
            {
                bool isConnected = await _dbContext.TestConnectionAsync();
                if (isConnected)
                {
                    await DisplayAlert("✅ Success", "Connected to Cosmos DB!", "OK");
                    return true;
                }
                else
                {
                    await DisplayAlert("❌ Error", $"Failed to connect to Cosmos DB.", "OK");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Database connection error: {ex.Message}");
                await DisplayAlert("Error", $"Database connection error: {ex.Message}", "OK");
                return false;
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                var users = await _dbContext.Users.ToListAsync();

                Users.Clear();
                foreach (var user in users)
                {
                    Debug.WriteLine($"Loaded user: {user.Id}, ID: {user.UserName}");
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Failed to load conditions: {ex.Message}");
                await DisplayAlert("Error", $"Failed to load conditions: {ex.Message}", "OK");
            }
        }


        private void OnLoginClicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new AppShell();
        }

        private async void OnNewClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}