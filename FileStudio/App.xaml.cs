using FileStudio.Ai;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using FileStudio.FileManagement;
using FileStudio.ViewModels; // Add this using statement

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FileStudio
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register the AI client and service
            services.AddSingleton<IAiClient, GeminiAiClient>(_ => new GeminiAiClient(apiKey: Environment.GetEnvironmentVariable("GEMINI_API_KEY")));
            services.AddTransient<AiService>();
            services.AddTransient<IAiService>(provider => provider.GetRequiredService<AiService>());

            // register prompt generator
            services.AddTransient<IPromptGenerator, FilePromptGenerator>();

            // *** Register the new File Service ***
            services.AddSingleton<IFileService, FileService>(); 

            // Register MainWindow or ViewModels if needed
            // services.AddTransient<MainWindow>();
            services.AddTransient<MainWindowViewModel>(); // Register the ViewModel

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // m_window = new MainWindow(); // Old way
            m_window = new MainWindow(ServiceProvider.GetRequiredService<MainWindowViewModel>()); // Inject ViewModel
            m_window.Activate();
        }

        private Window m_window;
    }
}
