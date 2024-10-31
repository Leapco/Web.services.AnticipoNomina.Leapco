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
                    var url = _configuration["paymentGateway:route"] + "/accounts/" + dataUser.Rows[0]["x_api_key_cobre"].ToString() + "?sensitive_data=true";

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
                    var url = _configuration["paymentGateway:route"] + "/accounts/acc_5ilrSi0jCu?sensitive_data=true";

                    // Agregar el token de autorización
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token_acces);

                    // Hacer la solicitud GET
                    var response = _httpClient.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        int balance = jsonObject.obtained_balance;
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
        public string PostCounterParty(string Token_acces, DataTable dataUser)
        {
            LogsModel logsModel = new LogsModel(_configuration);
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    // Endpoint de la API de Cobre V3
                    var url = _configuration["paymentGateway:route"] + "/counterparties";

                    // Serializar el objeto de solicitud a JSON
                    var jsonContent = JsonConvert.SerializeObject(this.GetJsonCouterParty(dataUser));
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    // Agregar el token de autorización
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token_acces);

                    // Hacer la solicitud POST
                    var response = _httpClient.PostAsync(url, content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        string id = jsonObject.id;
                        return id;
                    }
                    else
                    {
                        var errorContent = response.Content.ReadAsStringAsync().Result;

                        LogRequest logRequest = new LogRequest()
                        {
                            Origen = "PostCounterParty",
                            Request_json = response.Content.ReadAsStringAsync().Result.ToString(),
                            Observacion = "Error registrando la cuenta del usuario en cobre"
                        };
                        logsModel.PostLog(logRequest);
                        return null;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }         
        public CounterpartyRequest GetJsonCouterParty(DataTable dataUser)
        {
            CounterpartyRequest counterpartyRequest = new CounterpartyRequest();
            Metadata metadata = new Metadata();
            counterpartyRequest.geo = "col";

            if (!string.IsNullOrEmpty(dataUser.Rows[0]["accountType"].ToString()))
                counterpartyRequest.type = dataUser.Rows[0]["accountType"].ToString();
            else
                return null;

            var documentNumber = dataUser.Rows[0]["documentNumber"].ToString();
            if (!string.IsNullOrEmpty(documentNumber))
                counterpartyRequest.alias = documentNumber + " - 1";
            else
                return null;

            if (!string.IsNullOrEmpty(dataUser.Rows[0]["accountNumber"].ToString()))
                metadata.account_number = dataUser.Rows[0]["accountNumber"].ToString();
            else
                return null;

            var email = dataUser.Rows[0]["email"].ToString();
            if (!string.IsNullOrEmpty(email))
                metadata.counterparty_email = email.Trim();
            else
                return null;

            var phone = dataUser.Rows[0]["phone"].ToString();
            if (!string.IsNullOrEmpty(phone))
                metadata.counterparty_phone = phone;
            else
                return null;

            var document = dataUser.Rows[0]["documentType"].ToString();
            if (!string.IsNullOrEmpty(document))
                metadata.counterparty_id_type = document;
            else
                return null;

            var name = dataUser.Rows[0]["name"].ToString();
            var lastName = dataUser.Rows[0]["lastName"].ToString();
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(lastName))
            {
                metadata.counterparty_fullname = name + " " + lastName;
            }                
            else
                return null;

            metadata.counterparty_id_number = documentNumber;

            if (!string.IsNullOrEmpty(dataUser.Rows[0]["bankCode"].ToString()))
                metadata.beneficiary_institution = dataUser.Rows[0]["bankCode"].ToString();
            else
                return null;


            counterpartyRequest.metadata = metadata;

            return counterpartyRequest;
        }
        public CounterpartyContent GetCounterPartyID(string Token_acces, string idCounterParty)
        {
            LogsModel logsModel = new LogsModel(_configuration);
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    // Endpoint de la API de Cobre V3
                    var url = _configuration["paymentGateway:route"] + "/counterparties?sensitive_data=true";

                    // Agregar el token de autorización
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token_acces);

                    // Hacer la solicitud GET
                    var response = _httpClient.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = response.Content.ReadAsStringAsync().Result;

                        // Deserializar la respuesta completa
                        CounterpartyResponse counterpartyResponse = JsonConvert.DeserializeObject<CounterpartyResponse>(responseBody);
                        List<CounterpartyContent> counterpartyList = counterpartyResponse.Contents;
                        
                        // Buscar datos del destinatario por id
                        CounterpartyContent counterparty = counterpartyList.FirstOrDefault(c => c.id == idCounterParty);

                        return counterparty;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public bool DELETECounterPartyID(string Token_acces, string idCounterParty)
        {
            LogsModel logsModel = new LogsModel(_configuration);
            try
            {
                using (var _httpClient = new HttpClient())
                {
                    // Endpoint de la API de Cobre V3
                    var url = _configuration["paymentGateway:route"] + $"/counterparties/{idCounterParty}";

                    // Agregar el token de autorización
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token_acces);

                    // Hacer la solicitud GET
                    var response = _httpClient.DeleteAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}