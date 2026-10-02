using CardapioInteravito.MOBILE.Views;

namespace CardapioInteravito.MOBILE
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginView), typeof(LoginView));
            Routing.RegisterRoute(nameof(RestaurantesView), typeof(RestaurantesView));
            Routing.RegisterRoute(nameof(MontarCardapioView), typeof(MontarCardapioView));
        }
    }
}
