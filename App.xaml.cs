namespace HandWStat
{
    public partial class App : Application
    {
        private readonly Services.Updates.IUpdateCheckCoordinator _updateCoordinator;

        public App(Services.Updates.IUpdateCheckCoordinator updateCoordinator)
        {
            InitializeComponent();
            _updateCoordinator = updateCoordinator;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "HandWStat" };
            window.Activated += HandleWindowResumed;
            window.Resumed += HandleWindowResumed;
            return window;
        }

        private void HandleWindowResumed(object? sender, EventArgs eventArgs) =>
            _ = _updateCoordinator.CheckOnResumeAsync();
    }
}
