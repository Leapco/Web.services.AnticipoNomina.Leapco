using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;

namespace WebServicesAnticiposNomina.Models.PaymentGateway
{
    public class ApiCobre
    {
        private readonly IConfiguration _configuration;
        public ApiCobre(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string PostAuthToken(PaymentClass paymentClass)
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
                string? xapikey = _configuration["paymentGateway:x-api-key"];
                string route = _configuration["paymentGateway:route"] + "/api-auth/v1/util/tokens";

                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", xapikey);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeader);

                try
                {
                    var response = httpClient.PostAsync(route, requestContent).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().Result;

                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        string accessToken = jsonObject.access_token;

                        return accessToken;
                    }
                    else
                        throw new Exception($"Failed to get auth token. Status code: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Utilities utilities = new(_configuration);
                    //utilities.SendSms("3007185717", ex.Message);
                    throw;
                }
            }
        }
        public string PostPayment(string Token, PaymentClass paymentClass)
        {
            using (var _httpClient = new HttpClient())
            {
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _configuration["paymentGateway:x-api-key"]);
                _httpClient.DefaultRequestHeaders.Add("X-APIGW-AUTH", Token);

                var jsonRequest = @"{
                      ""controlRecord"": 1,
                      ""noveltyDetails"": [
                          {
                              ""type"": ""TRANSFER"",
                              ""totalAmount"": " + paymentClass.noveltyDetails[0].totalAmount + @",
                              ""description"": ""Pago de Prueba 02"",
                              ""descriptionExtra1"": """",
                              ""descriptionExtra2"": """",
                              ""descriptionExtra3"": """",
                              ""dueDate"": """",
                              ""reference"": """ + paymentClass.noveltyDetails[0].reference + @""",
                              ""beneficiary"": {
                                  ""documentType"": """ + paymentClass.noveltyDetails[0].beneficiary.documentType + @""",
                                  ""documentNumber"": """ + paymentClass.noveltyDetails[0].beneficiary.documentNumber + @""",
                                  ""name"": """ + paymentClass.noveltyDetails[0].beneficiary.name + @""",
                                  ""lastName"": """ + paymentClass.noveltyDetails[0].beneficiary.lastName + @""",
                                  ""email"": """ + paymentClass.noveltyDetails[0].beneficiary.email + @""",
                                  ""phone"": """ + paymentClass.noveltyDetails[0].beneficiary.phone + @""",
                                  ""bankInfo"": {
                                      ""bankCode"": """ + paymentClass.noveltyDetails[0].beneficiary.bankInfo.bankCode + @""",
                                      ""accountType"": """ + paymentClass.noveltyDetails[0].beneficiary.bankInfo.accountType + @""",
                                      ""accountNumber"": """ + paymentClass.noveltyDetails[0].beneficiary.bankInfo.accountNumber + @"""
                                  }
                              }
                          }
                      ]
                  }";

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                string route = _configuration["paymentGateway:route"] + "/workplace-bank-instruction/v2/task/novelties";

                // Realiza la solicitud POST de forma síncrona
                var response = _httpClient.PostAsync(route, content).Result;

                // Lee y retorna el contenido de la respuesta
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result; 
                    dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string uuid = jsonObject.uuid;

                    return uuid;
                }
                else
                {
                    throw new Exception($"Failed to get auth token. Status code: {response.StatusCode}");
                }
            }
        }

        public int GetBalanceBank(string Token)
        {
            using (var _httpClient = new HttpClient())
            {
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _configuration["paymentGateway:x-api-key"]);
                _httpClient.DefaultRequestHeaders.Add("X-APIGW-AUTH", Token);

                string route = _configuration["paymentGateway:route"] + "/workplace-bank-account/v1/entity/workplace-bank-balance";

                // Realiza la solicitud GET de forma asíncrona
                var response = _httpClient.GetAsync(route).Result;

                if (response.IsSuccessStatusCode)
                {
                    // Lee y retorna el contenido de la respuesta si la solicitud fue exitosa
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    int balance = jsonObject.balance;

                    return balance;
                }
                else
                {
                    throw new Exception($"Failed to get balance. Status code: {response.StatusCode}");
                }
            }
        }


        //public string PostPaymentx(string Token, PaymentClass paymentClass)
        //{
        //    var request = (HttpWebRequest)WebRequest.Create(_configuration["paymentGateway:route"] + "/workplace-bank-instruction/v2/task/novelties");
        //    request.Method = "POST";
        //    request.Headers.Add("X-API-KEY", _configuration["paymentGateway:x-api-key"]);
        //    request.Headers.Add("X-APIGW-AUTH", Token);
        //    request.Headers.Add("X-CORRELATION-ID", "");
        //    request.Accept = "application/json";
        //    request.ContentType = "application/json";

        //    var jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(paymentClass);
        //    var requestBody = Encoding.UTF8.GetBytes(jsonRequest);

        //    using (var requestStream = request.GetRequestStream())
        //    {
        //        requestStream.Write(requestBody, 0, requestBody.Length);
        //    }

        //    try
        //    {
        //        using (var response = (HttpWebResponse)request.GetResponse())
        //        {
        //            using (var responseStream = response.GetResponseStream())
        //            {
        //                using (var reader = new StreamReader(responseStream))
        //                {
        //                    var responseContent = reader.ReadToEnd();
        //                    return responseContent;
        //                }
        //            }
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        var errorResponse = ex.Response as HttpWebResponse;
        //        if (errorResponse != null)
        //        {
        //            var statusCode = errorResponse.StatusCode;
        //            var statusDescription = errorResponse.StatusDescription;
        //            // Puedes manejar el error de acuerdo a tus necesidades aquí
        //        }
        //        throw;
        //    }
        //}

        public async Task<string> PostPaymentAsync(string Token)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://api-ext.qa.cobre.co/workplace-bank-instruction/v2/task/novelties"),
                Headers =
                {
                    { "X-API-KEY", _configuration["paymentGateway:x-api-key"]},
                    { "X-APIGW-AUTH", Token},
                    { "X-CORRELATION-ID", "123" },
                    { "Accept", "application/json" },
                },
                Content = new StringContent("{\r\n    \"controlRecord\": 1,\r\n    \"noveltyDetails\": [\r\n        {\r\n            \"type\": \"TRANSFER\",\r\n            \"totalAmount\": 1000,\r\n            \"description\": \"Pago de Prueba 02\",\r\n            \"descriptionExtra1\": \"\",\r\n            \"descriptionExtra2\": \"\",\r\n            \"descriptionExtra3\": \"\",\r\n            \"dueDate\": \"\",\r\n            \"reference\": \"Referencia-02\",\r\n            \"beneficiary\": {\r\n                \"documentType\": \"CC\",\r\n                \"documentNumber\": \"111111\",\r\n                \"name\": \"PEPITO\",\r\n                \"lastName\": \"PERES\",\r\n                \"email\": \"pepito.perez@yopmail.com\",\r\n                \"phone\": \"3151111111\",\r\n                \"bankInfo\": {\r\n                    \"bankCode\": \"1007\",\r\n                    \"accountType\": \"CH\",\r\n                    \"accountNumber\": \"111111111\"\r\n                }\r\n            }\r\n        }\r\n    ]\r\n}")
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
            using (var response = await client.SendAsync(request))
            {
                return response.ToString();
                //response.EnsureSuccessStatusCode();
                //var body = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(body);
            }
            return null;
        }
    }
}