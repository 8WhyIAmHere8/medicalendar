using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using medi1.Pages.AddEntryPageFolder;

namespace medi1.Pages.ConditionsPage;
public partial class AddConditionPopup : ContentPage
{
    public AddConditionPopup(string relatedSymptom)
    {
        InitializeComponent();
        BindingContext = new AddConditionPopupViewModel(relatedSymptom);
    }

    public AddConditionPopup() : this(string.Empty) { }
}
