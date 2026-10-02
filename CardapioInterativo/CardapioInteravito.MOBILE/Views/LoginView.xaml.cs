using CardapioInteravito.MOBILE.Helpers;
using CardapioInteravito.MOBILE.Service;

namespace CardapioInteravito.MOBILE.Views;

public partial class LoginView : ContentPage
{

    private int pinAleatorio;
    public LoginView()
    {
        InitializeComponent();
    }

    private async void btnLogin_Clicked(object sender, EventArgs e)
    {

        if (string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            lblError.IsVisible = true;
            lblError.Text = "Por favor, preencha seu email.";
            return;
        }

        if (string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            lblError.IsVisible = true;
            lblError.Text = "Por favor, preencha sua senha.";
            return;
        }
        string email = txtEmail.Text.Trim();
        string senha = txtSenha.Text.Trim();

        lblError.IsVisible = false;

        var loginRequest = new LoginRequestDto();
        loginRequest.Email = email;
        loginRequest.Senha = senha;
        var response = await ApiService.PostAsync<LoginRequestDto, LoginResponseDto>("auth/login", loginRequest);

        if (response == null)
        {
            lblError.IsVisible = true;
            lblError.Text = ApiService.Erro;
            return;
        }

        pinAleatorio = Random.Shared.Next(100, 10000);


        SessaoUsuario.UsauarioId = response.IdUsuario;
        SessaoUsuario.NomeUsuario = response.NomeUsuario;
        SessaoUsuario.PerfilId = response.PerfilId;
        SessaoUsuario.EmailUsuario = response.EmailUsuario;

        lblError.IsVisible = false;
        pnlPin.IsVisible = true;
        pnlLogin.IsVisible = false;
        lblError.Text = "";
        pinGerado.Text = pinAleatorio.ToString();
    }

    private async void btnConfirmarPin_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtPin.Text))
        {
            lblError.IsVisible = true;
            lblError.Text = "Por favor, preencha o campo do pin.";
            return;
        }

        if (txtPin.Text != pinAleatorio.ToString())
        {
            lblError.IsVisible = true;
            lblError.Text = "Pin ERRADO. Por favor, insira o pin que aparece na sua tela.";
            return;

        }

        lblError.IsVisible = false;
        lblOk.IsVisible = true;
        lblOk.Text = "Pin confirmado com sucesso. Login efetuado. Você será redirecionado para a tela principal";
        await Shell.Current.GoToAsync(nameof(RestaurantesView));

    }
    public record LoginRequestDto
    {
        public string Email { get; set; }
        public string Senha { get; set; }
    }
    public record LoginResponseDto
    {
        public int IdUsuario { get; set; }
        public string NomeUsuario { get; set; }
        public string CPF { get; set; }
        public int PerfilId { get; set; }
        public string EmailUsuario { get; set; }
    }
}


