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

public class ActivityLogNotesNull
{
    private readonly Mock<MedicalDbContext> _mockDbContext;
    private readonly Mock<DbSet<ActivityLog>> _mockActivityLogSet;
    private readonly List<ActivityLog> _savedActivityLogs = new();

    public ActivityLogNotesNull()
    {
        // Mock ActivityLog DbSet
        _mockActivityLogSet = new Mock<DbSet<ActivityLog>>();
        _mockActivityLogSet.Setup(m => m.AddAsync(It.IsAny<ActivityLog>(), It.IsAny<CancellationToken>()))
            .Callback<ActivityLog, CancellationToken>((al, ct) => _savedActivityLogs.Add(al))
            .ReturnsAsync((ActivityLog al, CancellationToken ct) => default);

        _mockDbContext = new Mock<MedicalDbContext>();
        _mockDbContext.Setup(db => db.ActivityEventLog).Returns(_mockActivityLogSet.Object);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    // Override PostSaveNavigation to avoid actual navigation during tests
    public class TestAddEntryViewModel : AddEntryViewModel
    {
        public TestAddEntryViewModel(MedicalDbContext dbContext) : base(dbContext) { }
        protected override Task PostSaveNavigation() => Task.CompletedTask;
    }

    [Fact]
    public async Task ConfirmEntry_ShouldAddActivityLog_WithoutNotes()
    {
        var viewModel = new TestAddEntryViewModel(_mockDbContext.Object)
        {
            SelectedEntryType = "Log Activity",
            SelectedActivity = new Activity { Name = "Yoga" },
            SelectedIntensity = "Low",
            ActivityDate = new DateTime(2025, 5, 15),
            ActivityHourDuration = 1,
            ActivityMinuteDuration = 0,
            Aggravated = false,
            EntryNotes = null // Intentionally blank
        };

        await viewModel.ConfirmEntryCommand.ExecuteAsync(null);

        Assert.Single(_savedActivityLogs);
        var savedLog = _savedActivityLogs.First();
        Assert.Equal("Yoga", savedLog.Name);
        Assert.Equal("Low", savedLog.Intensity);
        Assert.True(string.IsNullOrEmpty(savedLog.Notes)); 
    }
}
