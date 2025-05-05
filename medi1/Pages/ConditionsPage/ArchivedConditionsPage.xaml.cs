using medi1.Data;
using medi1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Windows.Input;

namespace medi1.Pages.ConditionsPage;

   public partial class ArchivedConditionsPage : ContentPage
{
    private readonly ViewModels.ArchivedConditionsViewModel _viewModel;

    public ArchivedConditionsPage()
    {
        InitializeComponent();
        _viewModel = new ViewModels.ArchivedConditionsViewModel();
        BindingContext = _viewModel;

        // Load data
        _viewModel.LoadArchivedConditionsCommand.Execute(null);
    }
}