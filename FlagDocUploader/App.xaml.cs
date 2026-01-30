using FlagDocUploader.Data;
using FlagDocUploader.Models;
using FlagDocUploader.Services;
using FlagDocUploader.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace FlagDocUploader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; } = null!;
        public IConfiguration Configuration { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Build configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Configure services
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Show main window
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddSingleton(Configuration);
            var appSettings = new AppSettings();
            Configuration.Bind(appSettings);
            services.Configure<AppSettings>(options =>
            {
                options.ConnectionStrings = appSettings.ConnectionStrings;
                options.Processing = appSettings.Processing;
            });

            // Logging
            services.AddLogging(configure =>
            {
                configure.AddSerilog();
            });

            // Database
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Services
            services.AddScoped<IFolderService, FolderService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IZipProcessingService, ZipProcessingService>();

            // ViewModels
            services.AddTransient<MainViewModel>();

            // Windows
            services.AddTransient<MainWindow>();
            ServiceProvider= services.BuildServiceProvider();
           var mainWindow= ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }

}
