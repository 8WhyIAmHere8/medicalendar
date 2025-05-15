using medi1.Data;
using medi1.Data.Models;
using medi1.Pages;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Windows.Input; // Only if ICommand is used

public class TaskEntryViewModelTests
{
    private readonly Mock<MedicalDbContext> _mockDbContext;
    private readonly Mock<DbSet<CalendarTask>> _mockTaskLog;
    private readonly List<CalendarTask> _taskList;
    private readonly TaskEntryViewModel _viewModel;

    public TaskEntryViewModelTests()
    {
        _taskList = new List<CalendarTask>();
        var queryable = _taskList.AsQueryable();

        _mockTaskLog = new Mock<DbSet<CalendarTask>>();
        _mockTaskLog.As<IQueryable<CalendarTask>>().Setup(m => m.Provider).Returns(queryable.Provider);
        _mockTaskLog.As<IQueryable<CalendarTask>>().Setup(m => m.Expression).Returns(queryable.Expression);
        _mockTaskLog.As<IQueryable<CalendarTask>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        _mockTaskLog.As<IQueryable<CalendarTask>>().Setup(m => m.GetEnumerator()).Returns(() => _taskList.GetEnumerator());

        _mockTaskLog.Setup(m => m.AddAsync(It.IsAny<CalendarTask>(), default))
            .Returns((CalendarTask task, System.Threading.CancellationToken _) =>
            {
                _taskList.Add(task);
                return ValueTask.FromResult((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CalendarTask>)null);
            });

        _mockDbContext = new Mock<MedicalDbContext>();
        _mockDbContext.Setup(db => db.TaskLog).Returns(_mockTaskLog.Object);
        _mockDbContext.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);

        _viewModel = new TaskEntryViewModel(_mockDbContext.Object);
    }

    [Fact]
    public async Task ConfirmTaskAsync_ShouldAddTaskToDb_WhenDescriptionIsValid()
    {
        // Arrange
        _viewModel.TaskDescription = "Test Task";
        bool alertWasShown = false;

        _viewModel.DisplayAlertRequested += (title, message, cancel) =>
        {
            alertWasShown = true;
            return Task.CompletedTask;
        };

        // Act
        var task = (Task)_viewModel.GetType()
            .GetMethod("ConfirmTaskAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(_viewModel, null);

        await task;


        // Assert
        Assert.Single(_taskList);
        _mockDbContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
        Assert.False(alertWasShown);
    }
}
