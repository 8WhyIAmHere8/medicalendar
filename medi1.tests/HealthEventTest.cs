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
using medi1.Services;

public class TestableAddEntryViewModel : AddEntryViewModel
{
    public TestableAddEntryViewModel(MedicalDbContext dbContext) : base(dbContext) { }

    protected override Task PostSaveNavigation() => Task.CompletedTask;
    protected override Task ShowError() => Task.CompletedTask;
}

public class HealthEventTests
{
    private readonly Mock<MedicalDbContext> _mockDbContext;
    private readonly Mock<DbSet<HealthEvent>> _mockHealthEventSet;
    private readonly List<HealthEvent> _savedEvents = new();

    public HealthEventTests()
    {
        // Prevent null reference in ViewModel constructor
        UserSession.Instance.Conditions = new List<medi1.Data.Models.Condition>();

        // Setup HealthEvent DbSet mock
        _mockHealthEventSet = new Mock<DbSet<HealthEvent>>();
        _mockHealthEventSet.Setup(m => m.AddAsync(It.IsAny<HealthEvent>(), It.IsAny<CancellationToken>()))
            .Callback<HealthEvent, CancellationToken>((he, ct) => _savedEvents.Add(he))
            .ReturnsAsync((HealthEvent he, CancellationToken ct) => default);

        // Setup DbContext mock with HealthEvent
        _mockDbContext = new Mock<MedicalDbContext>();
        _mockDbContext.Setup(db => db.HealthEvent).Returns(_mockHealthEventSet.Object);
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    [Fact]
    public async Task ConfirmEntry_ShouldAddHealthEventToDatabase()
    {
        var viewModel = new TestableAddEntryViewModel(_mockDbContext.Object)
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
            EntryNotes = "Sharp pain in the morning."
        };

        await viewModel.ConfirmEntryCommand.ExecuteAsync(null);

        Assert.Single(_savedEvents);
        var savedEvent = _savedEvents.First();
        Assert.Equal("Migraine", savedEvent.Title);
        Assert.Equal("2.5 hours", savedEvent.Duration);
        Assert.Equal(new System.DateTime(2024, 1, 1), savedEvent.StartDate);
        Assert.Equal(savedEvent.StartDate, savedEvent.EndDate);
        Assert.Equal("Sharp pain in the morning.", savedEvent.Notes);
    }
}


