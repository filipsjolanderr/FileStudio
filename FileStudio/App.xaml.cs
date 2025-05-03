using FileStudio.Ai;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using FileStudio.FileManagement;
using FileStudio.Repositories; // Add repository namespace
using FileStudio.ViewModels; // Add this using statement
using FileStudio.Communication; // Add Mediator namespace
using FileStudio.Communication.Handlers; // Add Handlers namespace
using FileStudio.Communication.Messages; // Add Messages namespace

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


            // register prompt generator
            services.AddTransient<IPromptGenerator, FilePromptGenerator>();

            // Register Repositories
            services.AddSingleton<IFileRepository, FileRepository>();
            services.AddSingleton<IAiRepository, AiRepository>();

            // Register Services (now depending on repositories)
            services.AddSingleton<IFileService, FileService>();
            // Update AiService registration if it was singleton before, or keep as transient
            // Assuming AiService should be transient as before
            services.AddTransient<AiService>(); 
            services.AddTransient<IAiService>(provider => provider.GetRequiredService<AiService>()); 

            // Register Mediator and its implementation
            services.AddSingleton<IMediator, Mediator>();

            // Register Request Handlers
            // Use AddTransient for handlers unless specific state needs to be maintained
            services.AddTransient<IRequestHandler<PickFolderRequest, PickFolderResponse>, PickFolderHandler>();
            services.AddTransient<IRequestHandler<GenerateResponseRequest, GenerateResponseResponse>, GenerateResponseHandler>();
            services.AddTransient<IRequestHandler<RenameFilesRequest, RenameFilesResponse>, RenameFilesHandler>();

            // Register MainWindow or ViewModels if needed
            // services.AddTransient<MainWindow>();
            services.AddTransient<MainWindowViewModel>(); // Register the ViewModel

            ServiceProvider = services.BuildServiceProvider();

            // Register handlers with the Mediator instance after the provider is built
            // This is a common pattern, but alternatives exist (like scanning assemblies)
            var mediator = ServiceProvider.GetRequiredService<IMediator>() as Mediator; // Cast to concrete Mediator to access registration methods
            if (mediator != null)
            {
                mediator.RegisterHandler<PickFolderRequest, PickFolderResponse>(() => ServiceProvider.GetRequiredService<IRequestHandler<PickFolderRequest, PickFolderResponse>>());
                mediator.RegisterHandler<GenerateResponseRequest, GenerateResponseResponse>(() => ServiceProvider.GetRequiredService<IRequestHandler<GenerateResponseRequest, GenerateResponseResponse>>());
                mediator.RegisterHandler<RenameFilesRequest, RenameFilesResponse>(() => ServiceProvider.GetRequiredService<IRequestHandler<RenameFilesRequest, RenameFilesResponse>>());
                // Register notification handlers here if any are created
            }
            else
            {
                // Handle error: Mediator could not be resolved or cast
                throw new InvalidOperationException("Could not register handlers with the Mediator.");
            }
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
