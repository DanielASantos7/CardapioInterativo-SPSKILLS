using CardapioInteravito.MOBILE.Views;

namespace CardapioInteravito.MOBILE
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new SplashScreenView();
        }

    }
}