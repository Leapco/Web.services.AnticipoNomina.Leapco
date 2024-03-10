using System.Text;

namespace WebServicesAnticiposNomina.Models.PaymentGateway
{
    public class ApiCobre
    {
        private readonly IConfiguration _configuration;
        public ApiCobre(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetAuthToken()
        {
            using (var httpClient = new HttpClient())
            {
                // Configura los headers y datos para la solicitud POST
                var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", _configuration["paymentGateway:grant_type"] }
                });

                // Construye las credenciales en el formato correcto para la autenticación básica HTTP
                string credentials = $"{_configuration["paymentGateway:Username"]}:{_configuration["paymentGateway:Password"]}";
                string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                string authorizationHeader = $"Basic {base64Credentials}";

                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", _configuration["paymentGateway:x-api-key"]);

                // Realiza la solicitud POST de forma síncrona
                var response = httpClient.PostAsync(_configuration["paymentGateway:route"], requestContent).Result;

                // Lee y retorna el contenido de la respuesta
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    return responseContent;
                }
                else
                {
                    throw new Exception($"Failed to get auth token. Status code: {response.StatusCode}");
                }
            }
        }
    }
}