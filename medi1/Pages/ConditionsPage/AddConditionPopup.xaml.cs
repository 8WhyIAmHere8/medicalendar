using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace medi1.Pages.ConditionsPage;
public partial class AddConditionPopup : ContentPage
{
    public AddConditionPopup()
    {
        InitializeComponent();
        BindingContext = new AddConditionPopupViewModel();
    }
}
