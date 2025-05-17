using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using medi1.Pages.ConditionsPage.Interfaces;
using medi1.Data.Models;
using medi1.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Condition = medi1.Data.Models.Condition;

public class AddConditionPageTests
{
    private readonly Mock<IMedicalDbContext> _mockDbContext;
    private readonly Mock<IAlertService> _mockAlertService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly List<Condition> _mockConditions;

    public AddConditionPageTests()
    {
        _mockDbContext = new Mock<IMedicalDbContext>();
        _mockAlertService = new Mock<IAlertService>();
        _mockNavigationService = new Mock<INavigationService>();
       

        _mockConditions = new List<Condition>();
        _mockDbContext.Setup(m => m.Conditions).Returns(MockDbSet(_mockConditions).Object);
        _mockDbContext.Setup(m => m.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task AddCondition_WithValidName_AddsConditionAndNavigates()
    {
        // Arrange
        var vm = new AddConditionPopupViewModel(_mockDbContext.Object, _mockAlertService.Object, _mockNavigationService.Object);
        vm.NewConditionName = "Valid Condition";

        // Act
        vm.ConfirmAddCommand.Execute(null);

        // Assert
        _mockDbContext.Verify(db => db.Conditions.Add(It.Is<Condition>(c => c.Name == "Valid Condition")), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Once);
        _mockNavigationService.Verify(nav => nav.PopModalAsync(), Times.Once);
        _mockAlertService.Verify(alert => alert.ShowAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddCondition_WithEmptyOrWhitespaceName_ShowsValidationAlert(string invalidName)
    {
        // Arrange
        var vm = new AddConditionPopupViewModel(_mockDbContext.Object, _mockAlertService.Object, _mockNavigationService.Object);
        vm.NewConditionName = invalidName;

        // Act
        vm.ConfirmAddCommand.Execute(null);

        // Assert
        _mockAlertService.Verify(alert =>
            alert.ShowAlert("Validation", "Condition name cannot be empty.", "OK"), Times.Once);

        _mockDbContext.Verify(db => db.Conditions.Add(It.IsAny<Condition>()), Times.Never);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddCondition_WhenSaveFails_ShowsErrorAlert()
    {
        // Arrange
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<System.Threading.CancellationToken>())).ThrowsAsync(new Exception("Database error"));

        var vm = new AddConditionPopupViewModel(_mockDbContext.Object, _mockAlertService.Object, _mockNavigationService.Object);
        vm.NewConditionName = "Failing Condition";

        // Act
        vm.ConfirmAddCommand.Execute(null);

        // Assert
        _mockAlertService.Verify(alert =>
            alert.ShowAlert("Error", "Failed to save condition.", "OK"), Times.Once);
    }

    // Helper to mock DbSet<Condition>
    private Mock<DbSet<Condition>> MockDbSet(List<Condition> sourceList)
    {
        var queryable = sourceList.AsQueryable();
        var dbSet = new Mock<DbSet<Condition>>();

        dbSet.As<IQueryable<Condition>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSet.As<IQueryable<Condition>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<Condition>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<Condition>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        dbSet.Setup(d => d.Add(It.IsAny<Condition>())).Callback<Condition>(sourceList.Add);

        return dbSet;
    }
}
