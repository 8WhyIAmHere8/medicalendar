using medi1.Data.Models;
using medi1.ViewModels;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using medi1.Services;

public class ConditionsPageTests
{
    private readonly Mock<ConditionsViewModel> _mockViewModel;

    public ConditionsPageTests()
    {
        _mockViewModel = new Mock<ConditionsViewModel>();
    }

    [Fact]
    public void SelectingACondition_TriggersDataUpdate()
    {
        // Arrange
        var vm = new ConditionsViewModel();
        var condition1 = new medi1.Data.Models.Condition
        {
            Id = "cond1", Name = "Asthma", Medications = new List<string> { "Ventolin" }
        };
        var condition2 = new medi1.Data.Models.Condition
        {
            Id = "cond2", Name = "Diabetes", Medications = new List<string> { "Insulin" }
        };

        vm.Conditions.Add(condition1);
        vm.Conditions.Add(condition2);

        // Act
        vm.SelectedCondition = condition2;

        // Assert
        Assert.Equal("Diabetes", vm.SelectedCondition?.Name);
        Assert.Contains("Insulin", vm.SelectedCondition?.Medications);
    }

    [Fact]
    public async Task AddMedication_WithValidName_AddsToCollection()
   {
    var vm = new ConditionsViewModel(); 
    var condition = new medi1.Data.Models.Condition { Name = "Asthma", Medications = new List<string>() };
    vm.Conditions.Add(condition);
    vm.SelectedCondition = condition;
    vm.NewMedication = "Ventolin";

    await vm.AddMedicationCommand.ExecuteAsync(null);

    Assert.Single(vm.SelectedCondition.Medications);
    Assert.Contains("Ventolin", vm.SelectedCondition.Medications);
}

    [Fact]
    public async Task AddMedication_WithEmptyOrWhitespaceName_DoesNotAdd()
    {

        var vm = new ConditionsViewModel();
        var condition = new medi1.Data.Models.Condition { Name = "Asthma", Medications = new List<string>() };
        vm.Conditions.Add(condition);
        vm.SelectedCondition = condition;


        vm.NewMedication = "";
        await vm.AddMedicationCommand.ExecuteAsync(null);
        Assert.Empty(vm.SelectedCondition.Medications);


        vm.NewMedication = "   ";
        await vm.AddMedicationCommand.ExecuteAsync(null);
        Assert.Empty(vm.SelectedCondition.Medications);
    }
}

