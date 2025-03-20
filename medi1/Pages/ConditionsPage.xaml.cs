using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace medi1.Pages
{
    public partial class ConditionsPage : ContentPage
    {
        public ObservableCollection<string> Medications { get; set; }
        public string Notes { get; set; }

        public Command AddMedicationCommand { get; }
        public Command SaveNoteCommand { get; }

        public ConditionsPage()
        {
            InitializeComponent();
            Medications = new ObservableCollection<string> { "Ventolin - Inhaler", "Claritine - Antihistamine" };
            Notes = "Hey fever may be making it worse? Bring up with GP";

            AddMedicationCommand = new Command(AddMedication);
            SaveNoteCommand = new Command(SaveNote);

            BindingContext = this;
        }

        private void AddMedication()
        {
            Medications.Add("New Medication"); // Replace with user input
        }

        private void SaveNote()
        {
            // Save logic here, possibly to a database later
            DisplayAlert("Saved", "Your note has been saved.", "OK");
        }
    }
}
