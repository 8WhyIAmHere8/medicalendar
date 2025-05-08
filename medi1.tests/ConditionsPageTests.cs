using medi1.Data;
using medi1.Data.Models;
using medi1.Pages.ConditionsPage;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ConditionsPageTests
{
    private readonly Mock<MedicalDbContext> _mockDbContext;
    private readonly ConditionsPage _conditionsPage;

    public ConditionsPageTests()
    {
        _mockDbContext = new Mock<MedicalDbContext>();
        _conditionsPage = new ConditionsPage
        {
            BindingContext = null // Prevent binding issues during tests
        };
    }

    [Fact]
    public async Task LoadConditions_ShouldPopulateConditions_WhenDatabaseHasData()
    {
        // Arrange
        var mockConditions = new List<medi1.Data.Models.Condition>
        {
            new medi1.Data.Models.Condition { Id = "1", Name = "Condition 1" },
            new medi1.Data.Models.Condition { Id = "2", Name = "Condition 2" }
        };
        _mockDbContext.Setup(db => db.Conditions.Where(It.IsAny<System.Linq.Expressions.Expression<System.Func<medi1.Data.Models.Condition, bool>>>()).ToList())
                      .Returns(mockConditions);

        // Act
        await _conditionsPage.LoadConditions();

        // Assert
        Assert.Equal(2, _conditionsPage.Conditions.Count);
        Assert.Equal("Condition 1", _conditionsPage.Conditions[0].Name);
    }

    [Fact]
    public async Task AddMedication_ShouldAddMedicationToCondition_WhenValidInput()
    {
        // Arrange
        var condition = new medi1.Data.Models.Condition { Id = "1", Name = "Condition 1", Medications = new List<string>() };
        _conditionsPage.SelectedCondition = condition;
        _conditionsPage.NewMedication = "Medication 1";

        // Act
        await _conditionsPage.AddMedication();

        // Assert
        Assert.Contains("Medication 1", condition.Medications);
        Assert.Contains("Medication 1", _conditionsPage.Medications);
    }

    [Fact]
    public async Task OnArchiveCondition_ShouldArchiveCondition_WhenConditionIsSelected()
    {
        // Arrange
        var condition = new medi1.Data.Models.Condition { Id = "1", Name = "Condition 1", Archived = false };
        _conditionsPage.SelectedCondition = condition;

        // Act
        await _conditionsPage.OnArchiveCondition();

        // Assert
        Assert.True(condition.Archived);
        Assert.DoesNotContain(condition, _conditionsPage.Conditions);
    }
}
