using Newtonsoft.Json;
using System.Text;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.DataBase;
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

        public string PostAuthToken(string Token)
        {
            LogsModel logsModel = new LogsModel(_configuration);
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
                        return "false";
                }
                catch (Exception ex)
                {
                    LogRequest logRequest = new LogRequest()
                    {
                        Origen = "PostAuthToken",
                        Request_json = credentials,
                        Observacion = "Autenticacion Cobre"
                    };
                    logsModel.PostLog(logRequest);
                    return "false";
                }
            }
        }
        public ResponseCobre PostPayment(string Token, PaymentClass paymentClass)
        {
            ResponseCobre responseCobre = new();
            LogsModel logsModel = new LogsModel(_configuration);
            int Id_anticipo = int.Parse(paymentClass.noveltyDetails[0].reference.Trim().Replace("Id_Anticipo - ", ""));
            LogRequest logRequest = new LogRequest()
            {
                Origen = "PostPayment",
                Id_Anticipo = Id_anticipo
            };            

            using (var _httpClient = new HttpClient())
            {
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _configuration["paymentGateway:x-api-key"]);
                _httpClient.DefaultRequestHeaders.Add("X-APIGW-AUTH", Token);
                _httpClient.DefaultRequestHeaders.Add("X-CORRELATION-ID", Id_anticipo.ToString());

                var jsonRequest = @"{
                      ""controlRecord"": 1,
                      ""noveltyDetails"": [
                          {
                              ""type"": ""TRANSFER"",
                              ""totalAmount"": " + paymentClass.noveltyDetails[0].totalAmount + @",
                              ""description"": ""Anticipos de nomina"",
                              ""descriptionExtra1"": """",
                              ""descriptionExtra2"": """",
                              ""descriptionExtra3"": """",
                              ""dueDate"": """",
                              ""reference"": """ + paymentClass.noveltyDetails[0].reference.Trim() + @""",
                              ""beneficiary"": {
                                  ""documentType"": """ + paymentClass.noveltyDetails[0].beneficiary.documentType.Trim() + @""",
                                  ""documentNumber"": """ + paymentClass.noveltyDetails[0].beneficiary.documentNumber.Trim() + @""",
                                  ""name"": """ + paymentClass.noveltyDetails[0].beneficiary.name.Trim() + @""",
                                  ""lastName"": """ + paymentClass.noveltyDetails[0].beneficiary.lastName.Trim() + @""",
                                  ""email"": """ + paymentClass.noveltyDetails[0].beneficiary.email.Trim() + @""",
                                  ""phone"": """ + paymentClass.noveltyDetails[0].beneficiary.phone.Trim() + @""",
                                  ""bankInfo"": {
                                      ""bankCode"": """ + paymentClass.noveltyDetails[0].beneficiary.bankInfo.bankCode.Trim() + @""",
                                      ""accountType"": """ + paymentClass.noveltyDetails[0].beneficiary.bankInfo.accountType.Trim() + @""",
                                      ""accountNumber"": """ + (paymentClass.noveltyDetails[0].beneficiary.bankInfo.accountNumber).Trim() + @"""
                                  }
                              }
                          }
                      ]
                  }";

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                string route = _configuration["paymentGateway:route"] + "/workplace-bank-instruction/v2/task/novelties";
                HttpResponseMessage response = new();
                Utilities utilities = new(_configuration);

                try
                {
                    response = _httpClient.PostAsync(route, content).Result;
                }
                catch (Exception ex)
                {
                    logRequest.Request_json = jsonRequest;
                    logRequest.Observacion = ex.Message;
                    logsModel.PostLog(logRequest);

                    responseCobre.code = "400";
                    responseCobre.Message = "Error al consumir la api";
                    return responseCobre;
                }

                var responseContent = response.Content.ReadAsStringAsync().Result;
                dynamic? jsonObject = JsonConvert.DeserializeObject<dynamic>(responseContent);

                // Lee y retorna el contenido de la respuesta
                if (response.IsSuccessStatusCode)
                {
                    responseCobre.data = jsonObject.uuid;
                    responseCobre.code = "201";
                    responseCobre.Message = "Anticipo registrado";
                    responseCobre.jsonRequest = jsonRequest;
                }
                else
                {
                    if (response.StatusCode.Equals("400"))
                    {
                        string ResponseMessage = jsonObject.moreInfo[0].message;

                        for (int i = 0; i < jsonObject.moreInfo.length; i++)
                        {
                            ResponseMessage = jsonObject.moreInfo[i].message + " - ";
                        }
                        string Message = "Revisar datos personales";
                        logRequest.Request_json = ResponseMessage;
                        logRequest.Observacion = Message;
                        logsModel.PostLog(logRequest);

                        responseCobre.code = "204";
                        responseCobre.Message = Message;
                    }
                    else
                    {
                        responseCobre.code = response.StatusCode.ToString();
                        responseCobre.Message = "Ha ocurrido un error al procesar tu pago, intentalo mas tarde.";
                    }
                }
                return responseCobre;
            }
        }

        public int GetBalanceBank(string Token)
        {
            LogsModel logsModel = new LogsModel(_configuration);
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
                    LogRequest logRequest = new LogRequest()
                    {
                        Origen = "GetBalanceBank",
                        Request_json = response.Content.ReadAsStringAsync().Result.ToString(),
                        Observacion = "No hay saldo disponible en la pasarela de pago Cobre"
                    };
                    logsModel.PostLog(logRequest);
                    return 0;
                }
            }
        }
    }
}