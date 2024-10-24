using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.DataBase;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;

namespace WebServicesAnticiposNomina.Models.PaymentGateway
{
    public class ApiCobre_v3
    {
        private readonly IConfiguration _configuration;
        public ApiCobre_v3(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public ResponseCobre PostPayment(string Token, PaymentClass paymentClass, DataTable dataUser)
        {
            ResponseCobre responseCobre = new();
            LogsModel logsModel = new(_configuration);
            int Id_anticipo = int.Parse(paymentClass.noveltyDetails[0].reference.Trim().Replace("Id_Anticipo - ", ""));
            LogRequest logRequest = new LogRequest()
            {
                Origen = "PostPayment",
                Id_Anticipo = Id_anticipo
            };

            using (var _httpClient = new HttpClient())
            {
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", dataUser.Rows[0]["x_api_key_cobre"].ToString());
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
                    logRequest.Origen = logRequest.Origen + " Error";
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
        public string PostAuthToken(DataTable dataUser)
        {
            using (var _httpClient = new HttpClient())
            {
                // Endpoint de la API de Cobre v3
                var url = _configuration["paymentGateway:route"] + "/auth";

                // Crear el objeto JSON para el cuerpo de la solicitud
                var requestBody = new
                {
                    user_id = dataUser.Rows[0]["UserCobre"],
                    secret = dataUser.Rows[0]["ClaveCobre"]
                };

                // Convertir el objeto a JSON
                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    // Hacer la solicitud POST
                    var response = _httpClient.PostAsync(url, content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // Leer el contenido de la respuesta
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        string accessToken = jsonObject.access_token;

                        return accessToken;
                    }
                    else
                        return "false";
                }
                catch (Exception ex)
                {
                    LogsModel logsModel = new LogsModel(_configuration);
                    LogRequest logRequest = new LogRequest()
                    {
                        Origen = "PostAuthToken",
                        Request_json = "credentials", // Organizar credenciales de seccion 
                        Observacion = $"Autenticacion Cobre v3 - " + ex.Message,
                    };
                    logsModel.PostLog(logRequest);
                    return "false";
                }
            }
        }
        public string PostAuthToken_DEV()
        {
            using (var _httpClient = new HttpClient())
            {
                // Endpoint de la API de Cobre v3
                var url = _configuration["paymentGateway:route"] + "/auth";

                // Crear el objeto JSON para el cuerpo de la solicitud
                var requestBody = new
                {
                    user_id = "cli_qqor9ztpcp_1",
                    secret = "hn.uqyH?s5Yw0u"
                };

                // Convertir el objeto a JSON
                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                try
                {
                    // Hacer la solicitud POST
                    var response = _httpClient.PostAsync(url, content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        // Leer el contenido de la respuesta
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        string accessToken = jsonObject.access_token;

                        return accessToken;
                    }
                    else
                        return "false";
                }
                catch (Exception ex)
                {
                    LogsModel logsModel = new LogsModel(_configuration);
                    LogRequest logRequest = new LogRequest()
                    {
                        Origen = "PostAuthToken",
                        Request_json = "credentials",
                        Observacion = $"Autenticacion Cobre v3 - " + ex.Message,
                    };
                    logsModel.PostLog(logRequest);
                    return "false";
                }
            }
        }
        public int GetBalanceBank(string Token_acces, DataTable dataUser)
        {
            LogsModel logsModel = new LogsModel(_configuration);
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    // Endpoint de la API de Cobre V3
                    var url = _configuration["paymentGateway:route"] + "/accounts/" + dataUser.Rows[0]["x_api_key_cobre"].ToString();

                    // Agregar el token de autorización
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token_acces);

                    // Hacer la solicitud GET
                    var response = _httpClient.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
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
            catch (Exception)
            {
                return 0;
            }
        }
        public int GetBalanceBank_DEV(string Token_acces)
        {
            LogsModel logsModel = new LogsModel(_configuration);
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    // Endpoint de la API de Cobre V3
                    var url = _configuration["paymentGateway:route"] + "/accounts/acc_5ilrSi0jCu";

                    // Agregar el token de autorización
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token_acces);

                    // Hacer la solicitud GET
                    var response = _httpClient.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        int balance = jsonObject.balance;
;
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
            catch (Exception)
            {
                return 0;
            }
        }
    }
}