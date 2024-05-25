using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace PruebasGemini.Logica.Servicios
{
    public class IaService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IaService> _logger;
        private readonly string _apiBaseUrl = "https://generativelanguage.googleapis.com"; // URL base de la API
        private readonly string _apiKey = ""; // API Key

        public IaService(HttpClient httpClient, ILogger<IaService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string[]> GenerateQuestions(string texto)
        {
            try
            {
                // Texto fijo para solicitar la generación de preguntas
                string textoConPrompt = $"Generame 10 preguntas de este texto: {texto}";

                // Construye la solicitud con el texto proporcionado y otros parámetros necesarios
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = textoConPrompt
                                }
                            }
                        }
                    }
                };

                var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                // Agrega el endpoint específico al final de la URL base
                var requestUrl = $"{_apiBaseUrl}/v1/models/gemini-1.5-pro:generateContent?key={_apiKey}"; 

                // Realiza la solicitud HTTP POST
                var response = await _httpClient.PostAsync(requestUrl, requestContent);

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                var questions = new List<string>(); // Utilizamos una lista para almacenar las preguntas

                // Verificamos si la estructura de la respuesta es la esperada
                if (responseData != null && responseData["candidates"] != null && responseData["candidates"][0] != null
                    && responseData["candidates"][0]["content"] != null && responseData["candidates"][0]["content"]["parts"] != null
                    && responseData["candidates"][0]["content"]["parts"][0] != null && responseData["candidates"][0]["content"]["parts"][0]["text"] != null)
                {
                    // Obtenemos el texto de las preguntas
                    var text = responseData["candidates"][0]["content"]["parts"][0]["text"].ToString();

                    // Dividimos el texto en líneas
                    var lines = text.Split('\n');

                    // Recorremos las líneas y buscamos aquellas que comiencen con un número seguido de un punto
                    foreach (var line in lines)
                    {
                        // Si la línea empieza con un número seguido de un punto, la consideramos como una pregunta
                        if (Regex.IsMatch(line, @"^\d+\.\s"))
                        {
                            // Agregamos la pregunta a la lista
                            questions.Add(line.Trim());
                        }
                    }

                    // Verificamos si se encontraron preguntas
                    if (questions.Any())
                    {
                        return questions.ToArray();
                    }
                    else
                    {
                        _logger.LogError("No se encontraron preguntas en el texto.");
                        return null;
                    }
                }
                else
                {
                    _logger.LogError("La estructura de la respuesta de la API no es la esperada.");
                    return null;
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción al llamar a la API: {ex.Message}");
                return null;
            }
        }
    }

}
