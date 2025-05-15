// using medi1.Data;
// using medi1.Data.Models;
// using medi1.Pages;
// using Moq;
// using Xunit;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.Maui.Controls;
// using Microsoft.Maui.Controls.Xaml;
// using Microsoft.Maui.Graphics;

// public class Test1
// {
//     private readonly Mock<MedicalDbContext> _mockDbContext;
//     private readonly HomePage _homePage;
//     private readonly List<CalendarTask> mockTaskLog = new List<CalendarTask>();

//     public Test1()
//     {
//         _mockDbContext = new Mock<MedicalDbContext>();
//         var mockTask = new medi1.Data.Models.CalendarTask { id = "1", TaskId = "1", Description = "Test", CompletionStatus = false };

//         _mockDbContext.Setup(db => db.TaskLog.Add(It.IsAny<CalendarTask>()))
//                       .Callback<CalendarTask>(mockTask => mockTaskLog.Add(mockTask));

//         _homePage = new HomePage(_mockDbContext.Object)
//         {
//             BindingContext = null
//         };

//     }

//     [Fact]
//     public async Task OnConfirmTaskClicked_ShouldAddTask()
//     {
//         // Act
//         _homePage.OnConfirmTaskClicked(null, null);

//         // Assert: Check if a blank task was added
//         Assert.Single(mockTaskLog);
//         Assert.Equal("Test", mockTaskLog[0].Description);
//     }
// }