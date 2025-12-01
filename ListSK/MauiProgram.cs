using ListSK.Models;
using ListSK.ViewModels;
using ListSK.Views;
using Microsoft.Extensions.Logging;
namespace ListSK
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<MainListViewModel>();
            builder.Services.AddSingleton<ShopingListView>();
            builder.Services.AddSingleton<MainListView>();
            builder.Services.AddTransient<AddPageModel>();
            builder.Services.AddTransient<ShopingListView>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
