[Fact]
public async Task HomePageViewModel_LoadsAndMapsData()
{
    var options = new DbContextOptionsBuilder<MedicalDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
    using var context = new MedicalDbContext(options);
    // Arrange test data
    context.HealthEvent.Add(new HealthEvent { Id = "E1", Title = "Test", StartDate = DateTime.Today, EndDate = DateTime.Today });
    await context.SaveChangesAsync();
    var vm = new HomePageViewModel(context);

    // Act
    await vm.ReloadAllAsync();

    // Assert
    Assert.Contains(vm.HealthEvents, e => e.Id == "E1");
    Assert.True(vm.DaysInMonth.Any(d => d.Entries.Any(en => en.Text == "Test")));
}