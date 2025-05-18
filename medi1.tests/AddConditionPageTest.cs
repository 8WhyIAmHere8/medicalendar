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
using System.Threading;
using System.Linq.Expressions;
using Condition = medi1.Data.Models.Condition;
using Microsoft.EntityFrameworkCore.Query;

public class AddConditionPageTests
{
    private readonly Mock<IMedicalDbContext> _mockDbContext;
    private readonly Mock<IAlertService> _mockAlertService;
    private readonly Mock<INavigationService> _mockNavigationService;
    private readonly List<Condition> _mockConditions;
    private readonly Mock<DbSet<User>> _mockUserDbSet;
    private readonly User _mockUser;

    public AddConditionPageTests()
    {
        _mockDbContext = new Mock<IMedicalDbContext>();
        _mockAlertService = new Mock<IAlertService>();
        _mockNavigationService = new Mock<INavigationService>();

        _mockConditions = new List<Condition>();
        _mockDbContext.Setup(m => m.Conditions).Returns(MockDbSetAsync(_mockConditions).Object);
        _mockDbContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockUser = new User
        {
            Id = "user-123",
            Conditions = new List<string>()
        };

        _mockUserDbSet = new Mock<DbSet<User>>();
        var users = new List<User> { _mockUser }.AsQueryable();
        _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _mockUserDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => users.GetEnumerator());

        _mockDbContext.Setup(m => m.Users).Returns(_mockUserDbSet.Object);
    }

    private Mock<DbSet<Condition>> MockDbSetAsync(List<Condition> sourceList)
    {
        var queryable = sourceList.AsQueryable();

        var asyncEnumerable = new TestAsyncEnumerable<Condition>(queryable);
        var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();

        var dbSet = new Mock<DbSet<Condition>>();

        dbSet.As<IAsyncEnumerable<Condition>>()
            .Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => asyncEnumerator);

        dbSet.As<IQueryable<Condition>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Condition>(queryable.Provider));
        dbSet.As<IQueryable<Condition>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<Condition>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<Condition>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        dbSet.Setup(d => d.Add(It.IsAny<Condition>())).Callback<Condition>(sourceList.Add);

        return dbSet;
    }

    [Fact]
    public async Task AddCondition_WithValidName_AddsConditionAndNavigates()
    {
        UserSession.Instance.Id = "user-123";

        var vm = new AddConditionPopupViewModel(_mockDbContext.Object, _mockAlertService.Object, _mockNavigationService.Object);
        vm.NewConditionName = "Unit Test";
        await vm.AddConditionAsync();

        _mockDbContext.Verify(db => db.Conditions.Add(It.Is<Condition>(c => c.Name == "Unit Test")), Times.Once);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockNavigationService.Verify(nav => nav.PopModalAsync(), Times.Once);
        _mockAlertService.Verify(alert => alert.ShowAlert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddCondition_WithEmptyOrWhitespaceName_ShowsValidationAlert(string invalidName)
    {
        UserSession.Instance.Id = "user-123";
        var vm = new AddConditionPopupViewModel(_mockDbContext.Object, _mockAlertService.Object, _mockNavigationService.Object);
        vm.NewConditionName = invalidName;
        await vm.AddConditionAsync();

        _mockAlertService.Verify(alert =>
            alert.ShowAlert("Validation", "Condition name cannot be empty.", "OK"), Times.Once);

        _mockDbContext.Verify(db => db.Conditions.Add(It.IsAny<Condition>()), Times.Never);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddCondition_WhenSaveFails_ShowsErrorAlert()
    {
        UserSession.Instance.Id = "user-123";
        _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Database error"));

        var vm = new AddConditionPopupViewModel(_mockDbContext.Object, _mockAlertService.Object, _mockNavigationService.Object);
        vm.NewConditionName = "Failing Condition";

        await vm.AddConditionAsync();

        _mockAlertService.Verify(alert =>
            alert.ShowAlert("Error", "Failed to save condition.", "OK"), Times.Once);
    }

    [Fact]
    public async Task AddCondition_Duplicate_ShowsValidationAlert()
    {
        UserSession.Instance.Id = "user-123";
        var existingCondition = new Condition { Id = "cond-001", Name = "Duplicate", Archived = false };
        _mockConditions.Add(existingCondition);
        _mockUser.Conditions.Add("cond-001");

        var vm = new AddConditionPopupViewModel(_mockDbContext.Object, _mockAlertService.Object, _mockNavigationService.Object);
        vm.NewConditionName = "Duplicate";

        await vm.AddConditionAsync();

        _mockAlertService.Verify(alert =>
            alert.ShowAlert("Duplicate", "A condition with this name already exists.", "OK"), Times.Once);

        _mockDbContext.Verify(db => db.Conditions.Add(It.IsAny<Condition>()), Times.Never);
        _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // supporting test helpers
    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
        public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
        public T Current => _inner.Current;
    }

    internal class TestAsyncQueryProvider<TEntity> : Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression)
            => _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(expression);

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var result = Execute<TResult>(expression);
            return Task.FromResult(result);
        }

        TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
