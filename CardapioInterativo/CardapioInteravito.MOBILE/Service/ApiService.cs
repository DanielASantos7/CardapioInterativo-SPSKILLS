using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

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


        private static readonly JsonSerializerOptions _jsonOption = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
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

        /// <summary>
        /// Esse é o Get genérico
        /// </summary>
        /// <typeparam name="T">Um tipo de resposta genérica</typeparam>
        /// <param name="endpoint"> rota para fazermos o get </param>
        /// <returns>retorna o conteúdo que a rota retornar ou erro </returns>
        public static async Task<T?> GetAsync<T>(string endpoint)
        {
            Erro = string.Empty;

            try
            {
                HttpResponseMessage response = await Http.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<T>(json, _jsonOption);
                }

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

        public static async Task<bool> DeleteAsync(string endpoint)
        {
            Erro = string.Empty;

            try
            {
                HttpResponseMessage response = await Http.DeleteAsync(endpoint);


                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<bool>(json, _jsonOption);
                }

                Erro = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Credenciais inválidas.",
                    HttpStatusCode.BadRequest => "Dados preenchidos incorretamente",
                    HttpStatusCode.NotFound => "Endereço não encontrado na API",
                    _ => $"Erro no servidor (Código: {(int)response.StatusCode})"
                };
                return false;
            }
            catch (Exception ex)
            {
                Erro = $"Falha de Conexão: {ex.Message}";
                return false;
            }
        }

        public static async Task<bool> PutAsync<T>(string endpoint, T dados)
        {
            try
            {
                var json = JsonSerializer.Serialize(dados);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await Http.PutAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    Erro = string.Empty;
                    return true;
                }

                Erro = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Credenciais inválidas.",
                    HttpStatusCode.BadRequest => "Dados preenchidos incorretamente",
                    HttpStatusCode.NotFound => "Endereço não encontrado na API",
                    _ => $"Erro no servidor (Código: {(int)response.StatusCode})"
                };
                return false;
            }
            catch (Exception ex)
            {
                Erro = ex.Message;
                return false;
            }
        }
    }
}
