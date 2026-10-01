using System.Net;
using System.Net.Http.Json;

namespace CardapioInteravito.MOBILE.Service
{
    public static class ApiService
    {

        // Isso aqui serve para configurar a conexção Http
        // o Timeout é para ter um tempo entre a requisição e a resposta
        // para não travar caso a requisição seja muito pesada
        private static readonly HttpClient Http = new()
        {
            BaseAddress = new Uri("http://10.106.130.96:5000/api/"),
            Timeout = TimeSpan.FromSeconds(10)
        };

        // Essa string vai ser para exibir os possíveis erros que nossa aplicação pode gerar
        public static string Erro { get; private set; } = string.Empty;

        /// <summary>
        /// Esse é o post genérico
        /// </summary>
        /// <typeparam name="TReq">Tipo genérico para fazer o post</typeparam>
        /// <typeparam name="TRes">Tipo genérico de resposta que pode receber</typeparam>
        /// <param name="endpoint">caminho do endpoint da API</param>
        /// <param name="dados">conteúdo a a ser enviado no post da API</param>
        /// <returns></returns>
        public static async Task<TRes?> PostAsync<TReq, TRes>(string endpoint, TReq dados)
        {
            Erro = string.Empty;

            try
            {
                var response = await Http.PostAsJsonAsync(endpoint, dados);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<TRes>();

                Erro = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Credenciais inválidas.",
                    HttpStatusCode.BadRequest => "Dados preenchidos incorretamente",
                    HttpStatusCode.NotFound => "Endereço não encontrado na API",
                    _ => $"Erro no servidor (Código: {(int)response.StatusCode})"
                };
                return default;
            }
            catch (Exception ex)
            {
                Erro = $"Falha de Conexão: {ex.Message}";
                return default;
            }
        }
    }
}
