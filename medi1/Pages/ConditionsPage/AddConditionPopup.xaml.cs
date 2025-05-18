using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using medi1.Pages.ConditionsPage.Interfaces;
using medi1.Pages.ConditionsPage.Services;

using medi1.Pages.AddEntryPageFolder;

namespace medi1.Pages.ConditionsPage;
public partial class AddConditionPopup : ContentPage
{
    public AddConditionPopup(string relatedSymptom, string healthEventID)
    {
        InitializeComponent();

        var dbContext = new MedicalDbContext();
        var alertService = new AlertService();
        var navigationService = new NavigationService();

        BindingContext = new AddConditionPopupViewModel(relatedSymptom, healthEventID, dbContext, alertService, navigationService );
    }

    public AddConditionPopup() : this(string.Empty, string.Empty) { }
}
