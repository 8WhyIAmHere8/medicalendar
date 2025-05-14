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
    public void AddCondition_WithValidName_AddsToCollection()
    {
        var vm = new ConditionsViewModel();
        var initialCount = vm.Conditions.Count;
        var condition = new medi1.Data.Models.Condition { Name = "Hypertension" };

        vm.Conditions.Add(condition);

        Assert.Equal(initialCount + 1, vm.Conditions.Count);
        Assert.Contains(vm.Conditions, c => c.Name == "Hypertension");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddCondition_WithEmptyOrWhitespaceName_DoesNotAdd(string input)
    {
        var vm = new ConditionsViewModel();
        var initialCount = vm.Conditions.Count;

        var condition = new medi1.Data.Models.Condition { Name = "   "};
        vm.Conditions.Add(condition);
        

        Assert.Equal(initialCount, vm.Conditions.Count);
    }

    // [Fact]
    // public void AddCondition_WithDuplicateName_DoesNotAddDuplicate()
    // {
    //     var vm = new ConditionsViewModel();
    //     var condition = new medi1.Data.Models.Condition { Name = "Asthma" };
    //     vm.Conditions.Add(condition);

    //     var duplicate = new medi1.Data.Models.Condition { Name = "Asthma" };
    //     vm.Conditions.Add(duplicate);
        

    //     Assert.Equal(1, vm.Conditions.Count(c => c.Name == "Asthma"));
    // }
}

