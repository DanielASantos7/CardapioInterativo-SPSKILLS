namespace CardapioInteravito.MOBILE.Views;

public partial class SplashScreenView : ContentPage
{
    public SplashScreenView()
    {
        InitializeComponent();
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var cores = new[]
        {
            Color.FromArgb("#A4C2D6"),
            Color.FromArgb("#C75D4D"),
            Color.FromArgb("#7A8C69")
        };

        // Loop de 12 segundos
        for (int i = 0; i <= 12; i++)
        {
            //Delay de 1 segundo
            await Task.Delay(1000);

            // Atualiza a barra proporcionalmente (1/12 2/12 ... 12/12)
            pgBar.Progress = i / 12.0;

            // A cada 2 segundos
            if (i % 2 == 0)
            {
                int p = i / 2;
                Topo.BackgroundColor = cores[p % 3];
                Meio.BackgroundColor = cores[(p + 1) % 3];
                Baixo.BackgroundColor = cores[(p + 2) % 3];
            }
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Application.Current!.MainPage = new AppShell();
        });
    }
}