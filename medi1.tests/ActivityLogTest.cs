using medi1.Data;
using medi1.Data.Models;
using medi1.Pages.AddEntryPageFolder;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System;
using medi1.Services;


public class ActivityLogTests
{
    private readonly Mock<MedicalDbContext> _mockDbContext;
    private readonly Mock<DbSet<ActivityLog>> _mockActivityLogSet;
    private readonly Mock<DbSet<medi1.Data.Models.Condition>> _mockConditionSet;
    private readonly List<ActivityLog> _savedActivityLogs = new();

    public ActivityLogTests()
    {
        UserSession.Instance.Conditions = new List<medi1.Data.Models.Condition>();
        UserSession.Instance.ActivityLogs = new List<ActivityLog>();

        // Mock ActivityLog DbSet
        _mockActivityLogSet = new Mock<DbSet<ActivityLog>>();
        _mockActivityLogSet.Setup(m => m.AddAsync(It.IsAny<ActivityLog>(), It.IsAny<CancellationToken>()))
            .Callback<ActivityLog, CancellationToken>((al, ct) => _savedActivityLogs.Add(al))
            .ReturnsAsync((ActivityLog al, CancellationToken ct) => default);

        // Setup Condition with initialized Triggers list
        var condition = new medi1.Data.Models.Condition
        {
            Id = "cond1",
            Triggers = new List<string>()
        };

        // Mock Condition DbSet and setup FindAsync to return the condition above
        _mockConditionSet = new Mock<DbSet<medi1.Data.Models.Condition>>();
        _mockConditionSet.Setup(m => m.FindAsync(It.IsAny<object[]>()))
            .ReturnsAsync(condition);

        // Mock DbContext
        _mockDbContext = new Mock<MedicalDbContext>();
        _mockDbContext.Setup(db => db.ActivityEventLog).Returns(_mockActivityLogSet.Object);
        _mockDbContext.Setup(db => db.Conditions).Returns(_mockConditionSet.Object);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


    }

    public class TestAddEntryViewModel : AddEntryViewModel
    {
        public TestAddEntryViewModel(MedicalDbContext dbContext) : base(dbContext) { }

        protected override Task PostSaveNavigation() => Task.CompletedTask;
    }

    [Fact]
    public async Task ConfirmEntry_ShouldAddActivityLogToDatabase()
    {
        var viewModel = new TestAddEntryViewModel(_mockDbContext.Object)
        {
            SelectedEntryType = "Log Activity",
            SelectedActivity = new medi1.Data.Models.Activity { Name = "Running" },
            SelectedIntensity = "High",
            ActivityDate = new DateTime(2025, 5, 15),
            ActivityHourDuration = 1,
            ActivityMinuteDuration = 30,
            Aggravated = false,
            EntryNotes = "Felt good."
        };

        await viewModel.ConfirmEntryCommand.ExecuteAsync(null);

        Assert.Single(_savedActivityLogs);
        var savedLog = _savedActivityLogs.First();
        Assert.Equal("Running", savedLog.Name);
        Assert.Equal("High", savedLog.Intensity);
        Assert.Equal("Felt good.", savedLog.Notes);
    }
}

