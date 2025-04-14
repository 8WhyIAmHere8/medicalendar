using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Windows.Media.Capture.Core;
using medi1.Data;
using Microsoft.EntityFrameworkCore;
using medi1.Services;

namespace medi1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .Services.AddSingleton<UserSession>(); //Registers Usersession Class
            

            return builder.Build();
        }
    }
}
