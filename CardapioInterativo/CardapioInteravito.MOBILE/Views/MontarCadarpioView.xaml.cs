using CardapioInteravito.MOBILE.Service;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CardapioInteravito.MOBILE.Views;

[QueryProperty(nameof(RestauranteId), "restauranteId")]
public partial class MontarCardapioView : ContentPage
{
    public int RestauranteId { get; set; }

    private PratoDto? pratoArrastado;

    private ObservableCollection<PratoDto> pratosDisponiveis = new();
    private ObservableCollection<PratoDto> menuRestaurante = new();

    public MontarCardapioView()
    {
        InitializeComponent();

        cvPratosDisponiveis.ItemsSource = pratosDisponiveis;
        cvMenuRestaurante.ItemsSource = menuRestaurante;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await CarregarCardapio();
    }

    private async Task CarregarCardapio()
    {
        var resposta = await ApiService.GetAsync<CardapioDto>(
            $"cardapio/{RestauranteId}");

        if (resposta == null)
        {
            await DisplayAlert(
                "Erro",
                ApiService.Erro ?? "Erro ao carregar o cardápio.",
                "OK");

            return;
        }

        pratosDisponiveis.Clear();

        foreach (var prato in resposta.Disponiveis)
        {
            pratosDisponiveis.Add(prato);
        }

        menuRestaurante.Clear();

        foreach (var prato in resposta.Menu)
        {
            menuRestaurante.Add(prato);
        }
    }

    // VOLTAR
    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // ARRASTAR PRATO DISPONÍVEL
    private void PratoDisponivel_DragStarting(
        object sender,
        DragStartingEventArgs e)
    {
        if (sender is Element elemento)
        {
            pratoArrastado = elemento.BindingContext as PratoDto;
        }
    }

    // SOLTAR PRATO NO MENU
    private async void Menu_Drop(
        object sender,
        DropEventArgs e)
    {
        if (pratoArrastado == null)
            return;

        var prato = pratosDisponiveis.FirstOrDefault(
            p => p.Id == pratoArrastado.Id);

        if (prato == null)
        {
            pratoArrastado = null;
            return;
        }

        string? valorTexto = await DisplayPromptAsync(
            "Adicionar prato",
            "Informe o valor do prato:",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(valorTexto))
        {
            pratoArrastado = null;
            return;
        }

        valorTexto = valorTexto.Replace(',', '.');

        if (!double.TryParse(
                valorTexto,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out double valor))
        {
            await DisplayAlert(
                "Erro",
                "Informe um valor válido.",
                "OK");

            pratoArrastado = null;
            return;
        }

        if (valor <= 0)
        {
            await DisplayAlert(
                "Erro",
                "O valor deve ser maior que zero.",
                "OK");

            pratoArrastado = null;
            return;
        }

        var dados = new CardapioRequestDto
        {
            RestauranteId = RestauranteId,
            PratoId = prato.Id,
            Valor = valor
        };

        bool sucesso =
            await ApiService.PostAsync<CardapioRequestDto, bool>(
                "cardapio",
                dados) == true;

        if (!sucesso)
        {
            await DisplayAlert(
                "Erro",
                ApiService.Erro ?? "Erro ao adicionar o prato.",
                "OK");

            pratoArrastado = null;
            return;
        }

        pratosDisponiveis.Remove(prato);
        menuRestaurante.Add(prato);

        pratoArrastado = null;
    }

    // ARRASTAR PRATO DO MENU
    private void PratoMenu_DragStarting(
        object sender,
        DragStartingEventArgs e)
    {
        if (sender is Element elemento)
        {
            pratoArrastado = elemento.BindingContext as PratoDto;
        }
    }

    // SOLTAR PRATO NA LIXEIRA
    private async void Lixeira_Drop(
        object sender,
        DropEventArgs e)
    {
        if (pratoArrastado == null)
            return;

        var prato = menuRestaurante.FirstOrDefault(
            p => p.Id == pratoArrastado.Id);

        if (prato == null)
        {
            pratoArrastado = null;
            return;
        }

        bool sucesso = await ApiService.DeleteAsync(
            $"cardapio/{RestauranteId}/{prato.Id}");

        if (!sucesso)
        {
            await DisplayAlert(
                "Erro",
                ApiService.Erro ?? "Erro ao remover o prato.",
                "OK");

            pratoArrastado = null;
            return;
        }

        menuRestaurante.Remove(prato);
        pratosDisponiveis.Add(prato);

        pratoArrastado = null;
    }
}


// DTO DO PRATO
public class PratoDto
{
    public int Id { get; set; }

    public string Foto =>
        $"prato{((Math.Max(1, Id) - 1) % 5) + 1}.jpg";
}


// RESPOSTA DO CARDÁPIO
public class CardapioDto
{
    public List<PratoDto> Disponiveis { get; set; } = new();

    public List<PratoDto> Menu { get; set; } = new();
}


// REQUISIÇÃO PARA ADICIONAR PRATO
public class CardapioRequestDto
{
    public int RestauranteId { get; set; }

    public int PratoId { get; set; }

    public double Valor { get; set; }
}