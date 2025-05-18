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

public class HealthEventNotesNull
{
    private readonly Mock<MedicalDbContext> _mockDbContext;
    private readonly Mock<DbSet<HealthEvent>> _mockHealthEventSet;
    private readonly List<HealthEvent> _savedHealthEvents = new();

    public HealthEventNotesNull()
    {
        // Mock HealthEvent DbSet
        _mockHealthEventSet = new Mock<DbSet<HealthEvent>>();
        _mockHealthEventSet.Setup(m => m.AddAsync(It.IsAny<HealthEvent>(), It.IsAny<CancellationToken>()))
            .Callback<HealthEvent, CancellationToken>((he, ct) => _savedHealthEvents.Add(he))
            .ReturnsAsync((HealthEvent he, CancellationToken ct) => default);

        _mockDbContext = new Mock<MedicalDbContext>();
        _mockDbContext.Setup(db => db.HealthEvent).Returns(_mockHealthEventSet.Object);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    public class TestAddEntryViewModel : AddEntryViewModel
    {
        public TestAddEntryViewModel(MedicalDbContext dbContext) : base(dbContext) { }

        protected override Task PostSaveNavigation() => Task.CompletedTask;

        protected override Task ShowError() => Task.CompletedTask;

    }
    [Fact]
    public async Task ConfirmEntry_ShouldAddHealthEvent_WithoutNotes()
    {
        var viewModel = new TestAddEntryViewModel(_mockDbContext.Object)
        {
            SelectedEntryType = "Log Health Event",
            EventName = "Migraine",
            SelectedImpact = "5",
            SelectedDateRange = "Single Date",
            SingleDate = new System.DateTime(2024, 1, 1),
            IsFullDay = false,
            HourDuration = 2,
            MinuteDuration = 30,
            SelectedHealthRelation = "Independent Health Event",
            EntryNotes = null
        };

        await viewModel.ConfirmEntryCommand.ExecuteAsync(null);

        Assert.Single(_savedHealthEvents);
        var savedEvent = _savedHealthEvents.First();
        Assert.Equal("Migraine", savedEvent.Title);
        Assert.True(string.IsNullOrEmpty(savedEvent.Notes)); // Confirm notes are blank
    }
}
