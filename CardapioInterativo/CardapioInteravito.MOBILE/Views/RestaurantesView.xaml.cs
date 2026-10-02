using CardapioInteravito.MOBILE.Helpers;
using CardapioInteravito.MOBILE.Service;

namespace CardapioInteravito.MOBILE.Views;

public partial class RestaurantesView : ContentPage
{
    public RestaurantesView()
    {
        InitializeComponent();
        txtPesquisa.TextChanged += txtPesquisa_TextChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarRestaurantes();
    }

    private async Task CarregarRestaurantes(string? busca = null)
    {
        int usuarioId = SessaoUsuario.UsauarioId > 0 ? SessaoUsuario.UsauarioId : 1;
        int perfilId = SessaoUsuario.PerfilId > 0 ? SessaoUsuario.PerfilId : 1;

        string url = $"restaurante/todosRestaurantes?usuarioId={usuarioId}&perfilId={perfilId}";

        if (!string.IsNullOrWhiteSpace(busca) && busca.Trim().Length >= 2)
        {
            url += $"&busca={Uri.EscapeDataString(busca.Trim())}";
        }

        var lista = await ApiService.GetAsync<List<RestauranteResponseDTO>>(url);

        if (lista != null)
        {
            cvRestaurantes.ItemsSource = lista;
        }
        else
        {
            await DisplayAlertAsync("Erro de conexao", ApiService.Erro ?? "Nao foi possivel carregar os restaurantes", "OK");
        }
    }

    private async void txtPesquisa_TextChanged(object? sender, TextChangedEventArgs e)
    {
        string termo = e.NewTextValue?.Trim() ?? "";

        if (termo.Length >= 2 || termo.Length == 0)
        {
            await CarregarRestaurantes(termo);
        }
    }

    private async void SwipeItem_Invoked(object sender, EventArgs e)
    {
        if (SessaoUsuario.PerfilId != 1) return;

        if (sender is SwipeItem swipeItem && swipeItem.BindingContext is RestauranteResponseDTO restaurante)
        {
            bool confirmar = await DisplayAlertAsync(
                "Excluir Restaurante",
                $"Tem certeza de que deseja excluir '{restaurante.Nome}'?",
                "Sim", "Nao");

            if (confirmar)
            {
                await ExcluirRestaurante(restaurante.IdRestaurante);
            }
        }
    }

    private async void BtnAcaoPrincipal_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is RestauranteResponseDTO restaurante)
        {
            // 1. RESTAURAR (Se o restaurante estiver excluido)
            if (restaurante.IsExcluido)
            {
                bool restaurar = await DisplayAlertAsync("Restaurar", $"Deseja reativar '{restaurante.Nome}'?", "Sim", "Nao");
                if (restaurar)
                {
                    await RestaurarRestaurante(restaurante.IdRestaurante);
                }
                return;
            }

            // 2. PROPRIETARIO
            if (restaurante.IsProprietario)
            {
                await Shell.Current.GoToAsync(
                      $"{nameof(MontarCardapioView)}?restauranteId={restaurante.IdRestaurante}");
            }
            // 3. CLIENTE
            else
            {
                await DisplayAlertAsync("Ver Cardapio", $"Navegacao para Visualizar Cardapio do restaurante '{restaurante.Nome}' (Tela em desenvolvimento)", "OK");
            }
        }
    }

    private async Task RestaurarRestaurante(int idRestaurante)
    {
        bool sucesso = await ApiService.PutAsync($"restaurante/restaurar/{idRestaurante}", new { });

        if (sucesso)
        {
            await DisplayAlertAsync("Sucesso", "Restaurante restaurado com sucesso!", "OK");
            await CarregarRestaurantes();
        }
        else
        {
            await DisplayAlertAsync("Erro", ApiService.Erro ?? "Falha ao restaurar restaurante", "OK");
        }
    }

    private async Task ExcluirRestaurante(int idRestaurante)
    {
        bool sucesso = await ApiService.DeleteAsync($"restaurante/delete/{idRestaurante}");

        if (sucesso)
        {
            await DisplayAlertAsync("Sucesso", "Restaurante excluido com sucesso!", "OK");
            await CarregarRestaurantes();
        }
        else
        {
            await DisplayAlertAsync("Erro", ApiService.Erro ?? "Falha ao excluir restaurante", "OK");
        }
    }

    private async void TabPratos_Tapped(object sender, TappedEventArgs e)
    {
        await DisplayAlertAsync("Pratos", "Navegacao para a listagem geral de Pratos (Tela em desenvolvimento)", "OK");
    }
}
public class RestauranteResponseDTO
{
    public int IdRestaurante { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string TipoRestaurante { get; set; } = string.Empty;
    public int Avaliacao { get; set; }

    // CASAMENTO DIRETO COM O JSON DA API (bool IsDeleted)
    public bool IsDeleted { get; set; }

    // Regras de Dominio
    public bool IsExcluido => IsDeleted;
    public bool IsAtivo => !IsDeleted;
    public bool IsProprietario => SessaoUsuario.PerfilId == 1;

    // Interface Visual
    public double OpacidadeCard => IsExcluido ? 0.5 : 1.0;
    public bool MostrarOverlayInativo => IsExcluido;
    public bool PodeArrastar => IsProprietario && !IsExcluido;

    public string TextoBotaoCardapio => IsExcluido ? "Restaurar" : (IsProprietario ? "Montar Cardápio" : "Ver Cardápio");
    public string CorBotaoCardapio => IsExcluido ? "#8E8E93" : "#512BD4";

    public string CidadeTipo => $"{Cidade} - {TipoRestaurante}";
    public string ImageLocal => $"restaurante{((IdRestaurante - 1) % 4) + 1}.png";

    public List<string> EstrelasImages
    {
        get
        {
            var lista = new List<string>();
            for (int i = 1; i <= 5; i++)
                lista.Add(i <= Avaliacao ? "estrelaBlack.png" : "estrela.png");
            return lista;
        }
    }
}