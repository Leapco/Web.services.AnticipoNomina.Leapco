using Newtonsoft.Json;
using System.Data;
using System.Text.Json;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.DataBase;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;
using WebServicesAnticiposNomina.Models.PaymentGateway;

namespace WebServicesAnticiposNomina.Core
{
    public class ApiCobreCore
    {
        private readonly IConfiguration _configuration;

        public ApiCobreCore(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public ResponseCobre PostPaymentAdvance(DataTable dataUser)
        {
            ResponseCobre responseModels = new();
            try
            {
                responseModels.Message = "Token expirado";
                responseModels.code = "401";

                SecurityCore securityCore = new(_configuration);
                ApiCobre_v3 apiCobre = new(_configuration);

                Utilities utilities = new(_configuration);
                // autenticacion
                string TokenApi = apiCobre.PostAuthToken(dataUser);
                if (TokenApi != "false")
                {
                    // validar y consultar id cuenta (counterparty)
                    string id_cuenta_pasarela = this.GetDataAccountUser(dataUser, TokenApi);

                    if (id_cuenta_pasarela == null || id_cuenta_pasarela == "")
                    {
                        responseModels.Message = "Error datos personales - counterparty";
                        responseModels.code = "204";

                        LogsModel logsModel = new LogsModel(_configuration);
                        LogRequest logRequest = new LogRequest()
                        {
                            Origen = "GetDataAccountUser",
                            Observacion = "Respuesta del metodo: " + id_cuenta_pasarela,
                        };
                        logsModel.PostLog(logRequest);
                    }
                    else
                    {
                        // asignar id destinatario
                        dataUser.Rows[0]["id_cuenta_pasarela"] = id_cuenta_pasarela;

                        // Balance de la cuenta
                        long Balance = apiCobre.GetBalanceBank(TokenApi, dataUser);
                        long Balance_client = long.Parse(dataUser.Rows[0]["totalAmount"].ToString() + "00");
                        if (Balance > Balance_client)
                        {
                            // crear transacion 
                            //var paymant = PutPaymentClass(dataUser);
                            try
                            {
                                responseModels = apiCobre.PostPayment(TokenApi, dataUser);
                            }
                            catch (Exception ex)
                            {
                                responseModels.code = "205";
                                LogsModel logsModel = new LogsModel(_configuration);
                                LogRequest logRequest = new LogRequest()
                                {
                                    Origen = "PostPayment - error",
                                    Observacion = "Eror al registrar la transaccion en cobre " + ex.Message,
                                };
                                logsModel.PostLog(logRequest);
                                throw;
                            }
                        }
                        else
                        {
                            string msg = "Se ha intentado hacer un Anticipo pero no se cuenta con el saldo suficiente, solicita la recarga de tu saldo - Cliente :" + dataUser.Rows[0]["x_api_key_cobre"].ToString();
                            responseModels.Message = msg;
                            responseModels.code = "205";
                            _ = utilities.SendEmail(dataUser.Rows[0]["EmailClient"].ToString(), "SALDO INSUFICIENTE COBRE", msg, false, "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogsModel logsModel = new LogsModel(_configuration);
                LogRequest logRequest = new LogRequest()
                {
                    Origen = "PostPayment - cath",
                    Observacion = ex.Message,
                };
                logsModel.PostLog(logRequest);

                responseModels.Message = "Error interno";
                responseModels.code = "500";
            }
            return responseModels;
        }
        public PaymentClass? PutPaymentClass(DataTable dataUser)
        {
            List<NoveltyDetail>? noveltyDetailList = new();
            NoveltyDetail? noveltyDetail = new();
            Beneficiary beneficiary = new();
            BankInfo bankInfo = new();
            PaymentClass paymentClass = new();

            var document = dataUser.Rows[0]["documentType"].ToString();
            if (!string.IsNullOrEmpty(document))
                beneficiary.documentType = document;
            else
                return null;

            var documentNumber = dataUser.Rows[0]["documentNumber"].ToString();
            if (!string.IsNullOrEmpty(documentNumber))
                beneficiary.documentNumber = documentNumber;

            else
                return null;

            var name = dataUser.Rows[0]["name"].ToString();
            if (!string.IsNullOrEmpty(name))
                beneficiary.name = name;
            else
                return null;

            var lastName = dataUser.Rows[0]["lastName"].ToString();
            if (!string.IsNullOrEmpty(lastName))
                beneficiary.lastName = lastName;
            else
                return null;

            var email = dataUser.Rows[0]["email"].ToString();
            if (!string.IsNullOrEmpty(email))
                beneficiary.email = email;
            else
                return null;

            var phone = dataUser.Rows[0]["phone"].ToString();
            if (!string.IsNullOrEmpty(phone))
                beneficiary.phone = phone;
            else
                return null;

            if (!string.IsNullOrEmpty(dataUser.Rows[0]["bankCode"].ToString()))
                bankInfo.bankCode = dataUser.Rows[0]["bankCode"].ToString();
            else
                return null;

            if (!string.IsNullOrEmpty(dataUser.Rows[0]["accountType"].ToString()))
                bankInfo.accountType = dataUser.Rows[0]["accountType"].ToString();
            else
                return null;

            if (!string.IsNullOrEmpty(dataUser.Rows[0]["accountNumber"].ToString()))
                bankInfo.accountNumber = dataUser.Rows[0]["accountNumber"].ToString();
            else
                return null;

            var totalAmount1 = int.Parse(dataUser.Rows[0]["totalAmount"].ToString());

            beneficiary.bankInfo = bankInfo;
            noveltyDetail.totalAmount = totalAmount1;
            noveltyDetail.reference = "Id_Anticipo - " + dataUser.Rows[0]["id_anticipo"].ToString();
            noveltyDetail.beneficiary = beneficiary;
            noveltyDetailList.Add(noveltyDetail);
            paymentClass.controlRecord = 1;
            paymentClass.noveltyDetails = noveltyDetailList;

            return paymentClass;
        }
        public string WeebHookPayment_V2(TransactionRequest transactionRequest)
        {
            AdvanceCore advanceCore = new(_configuration);
            Utilities utilities = new(_configuration);
            AdvanceModel advanceModel = new(_configuration);
            AdvanceRequest advanceRequest = new();
            advanceRequest.uuid = transactionRequest.NoveltyUuid;
            // campo dinamica, se envia toda la respuesta del weebhook
            advanceRequest.AdvanceAmount = JsonConvert.SerializeObject(transactionRequest);
            string bodyEmail;

            try
            {
                if (transactionRequest.Status == "FINISHED")
                {
                    DataTable dataUser = advanceModel.PostAdvance(advanceRequest, 6);
                    if (dataUser.Rows[0]["state"].ToString() == "2")
                    {
                        return "205";
                    }
                    if (!advanceCore.CreateContract(dataUser)) advanceCore.CreateContract(dataUser);

                    bodyEmail = utilities.GetBodyEmailCode("", dataUser, 5);
                    var email = utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo consignado", bodyEmail, true,
                                    _configuration["route:pathContrato"] + $"\\{dataUser.Rows[0]["id_anticipo"]}.pdf");
                    return "201";
                }
                else
                {
                    advanceRequest.DescriptionsCobre = transactionRequest.DescriptionStatus;
                    DataTable dataUser = advanceModel.PostAdvance(advanceRequest, 7);
                    if (dataUser.Rows[0]["state"].ToString() == "2")
                    {
                        return "205";
                    }
                    //consultar el por que del rechazo
                    string code = "200";
                    if (dataUser.Rows[0]["state"].ToString() == "1")
                    {
                        bodyEmail = utilities.GetBodyEmailCode("", dataUser, 2);
                        var email = utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo rechazado", bodyEmail, true, "");
                        code = "204";
                    }

                    return code;
                }
            }
            catch (Exception ex)
            {
                return "500 " + ex.Message;
            }
        }
        public string WeebHookPayment(dynamic webHookRequest)
        {
            AdvanceCore advanceCore = new(_configuration);
            Utilities utilities = new(_configuration);
            AdvanceModel advanceModel = new(_configuration);
            AdvanceRequest advanceRequest = new();
            string? stateCobre = "", codeCobre = "";

            // convertir webHookRequest a JsonElement
            JsonElement jsonElement = (JsonElement)webHookRequest;

            // Verifica y accede a los campos
            if (jsonElement.TryGetProperty("content", out JsonElement contentElement) &&
                contentElement.TryGetProperty("id", out JsonElement idElement) &&
                contentElement.TryGetProperty("status", out JsonElement statusElement) &&
                statusElement.TryGetProperty("state", out JsonElement stateElement) &&
                statusElement.TryGetProperty("code", out JsonElement codeElement))
            {
                advanceRequest.uuid = idElement.GetString();
                stateCobre = stateElement.GetString();
                codeCobre = codeElement.GetString();
            }
            else
            {
                // Cuando es valido manual desde el postman
                if (jsonElement.TryGetProperty("id", out JsonElement idElement1) &&
                    jsonElement.TryGetProperty("status", out JsonElement statusElement1) &&
                    statusElement1.TryGetProperty("state", out JsonElement stateElement1) &&
                    statusElement1.TryGetProperty("code", out JsonElement codeElement1))
                {
                    advanceRequest.uuid = idElement1.GetString();
                    stateCobre = stateElement1.GetString();
                    codeCobre = codeElement1.GetString();
                }
            }

            // campo dinamica, se envia toda la respuesta del weebhook
            advanceRequest.AdvanceAmount = jsonElement.GetRawText();
            string bodyEmail;

            try
            {
                DataTable dataUser = new DataTable();
                if (stateCobre == "completed")
                {
                    dataUser = advanceModel.PostAdvance(advanceRequest, 6);
                    // Validar saldo en cobre
                    this.ISValidateAccountBalance(dataUser, jsonElement);

                    if (dataUser.Rows[0]["state"].ToString() == "2")
                    {
                        return "205";
                    }
                    if (!advanceCore.CreateContract(dataUser)) advanceCore.CreateContract(dataUser);

                    bodyEmail = utilities.GetBodyEmailCode("", dataUser, 5);
                    var email = utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo consignado", bodyEmail, true,
                                    _configuration["route:pathContrato"] + $"//{dataUser.Rows[0]["id_anticipo"]}.pdf");

                    //Validacion de webbhok por api
                    if (dataUser.Rows[0]["api_key"].ToString() == "API")
                    {
                        WebhookCore webhookCore = new(_configuration);
                        webhookCore.SendWebhook(int.Parse(dataUser.Rows[0]["id_anticipo"].ToString()));
                    }

                    return "201 consignado";
                }
                else
                {
                    // Directorio de errores se busca por el codigo
                    advanceRequest.DescriptionsCobre = GeterrorDictionary(codeCobre);
                    dataUser = advanceModel.PostAdvance(advanceRequest, 7);

                    // Validar saldo en cobre
                    this.ISValidateAccountBalance(dataUser, jsonElement);

                    //Se elimina la foto
                    string pathImagenClient = _configuration["route:pathPhotoAdvance"] + "\\" + dataUser.Rows[0]["id_anticipo"] + ".jpg";
                    File.Delete(pathImagenClient);

                    //Validacion de webbhok por api
                    if (dataUser.Rows[0]["api_key"].ToString() == "API")
                    {
                        WebhookCore webhookCore = new(_configuration);
                        webhookCore.SendWebhook(int.Parse(dataUser.Rows[0]["id_anticipo"].ToString()));
                    }

                    if (dataUser.Rows[0]["state"].ToString() == "2")
                    {
                        return "205";
                    }
                    //consultar el por que del rechazo
                    string code = "200";
                    if (dataUser.Rows[0]["state"].ToString() == "1")
                    {
                        bodyEmail = utilities.GetBodyEmailCode("", dataUser, 2);
                        var email = utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo rechazado", bodyEmail, true, "");
                        code = "204 - rechazado";
                    }

                    return code;
                }
            }
            catch (Exception ex)
            {
                return "500" + ex.Message;
            }
        }
        public string? GetDataAccountUser(DataTable dataUser, string Token)
        {
            string? destination_id = null;
            string? dataAccountUser = dataUser.Rows[0]["id_cuenta_pasarela"].ToString();
            try
            {
                AdvanceModel advanceModel = new(_configuration);
                ApiCobre_v3 apiCobre_V3 = new ApiCobre_v3(_configuration);
                if (dataAccountUser == null || dataAccountUser == "")
                {
                    // crear counterparty
                    destination_id = apiCobre_V3.PostCounterParty(Token, dataUser);

                    // Registrar id_cuenta_pasarela
                    AdvanceRequest advanceRequest = new()
                    {
                        ID = dataUser.Rows[0]["documentNumber"].ToString(),
                        uuid = destination_id
                    };
                    _ = advanceModel.PostAdvance(advanceRequest, 9);
                }
                else
                {
                    destination_id = dataAccountUser;
                    // Validar datos del emprado en cobre                    
                    CounterpartyContent counterpartyContent = apiCobre_V3.GetCounterPartyID(Token, dataAccountUser);
                    if (counterpartyContent != null)
                    {
                        DataRow dataUserRow = dataUser.Rows[0];
                        bool isFiel = IsCounterpartyFieldsMatching(counterpartyContent, dataUserRow);
                        if (!isFiel)
                        {
                            destination_id = CleanCounterparty(dataUser, Token);
                        }
                    }
                    else
                    {
                        destination_id = CleanCounterparty(dataUser, Token);
                    }
                }
            }
            catch (Exception ex)
            {
                destination_id = null;
                LogsModel logsModel = new LogsModel(_configuration);
                LogRequest logRequest = new LogRequest()
                {
                    Origen = "GetDataAccountUser",
                    Request_json = ex.Message,
                    Observacion = $"Error creando counter party: documento {dataUser.Rows[0]["documentNumber"]}"
                };
                logsModel.PostLog(logRequest);
            }
            return destination_id;
        }
        private string? CleanCounterparty(DataTable dataUser, string Token)
        {
            string? dataAccountUser = dataUser.Rows[0]["id_cuenta_pasarela"].ToString();
            AdvanceModel advanceModel = new(_configuration);
            ApiCobre_v3 apiCobre_V3 = new ApiCobre_v3(_configuration);
            DataRow dataUserRow = dataUser.Rows[0];

            // Eliminar counterparty que no coincide
            _ = apiCobre_V3.DELETECounterPartyID(Token, dataAccountUser);

            // Preparar solicitud de avance para eliminar cuenta asociada y recrearla
            AdvanceRequest advanceRequest = new()
            {
                ID = dataUserRow["documentNumber"].ToString()
            };

            _ = advanceModel.PostAdvance(advanceRequest, 10);

            // Limpiar el ID de la cuenta en pasarela y obtener un nuevo destinatario id
            dataUserRow["id_cuenta_pasarela"] = null;
            return this.GetDataAccountUser(dataUser, Token);
        }
        private bool IsCounterpartyFieldsMatching(CounterpartyContent counterpartyContent, DataRow dataUserRow)
        {
            return counterpartyContent.metadata.counterparty_id_number.Trim() == dataUserRow["documentNumber"].ToString().Trim() &&
                    counterpartyContent.metadata.counterparty_id_type.ToUpper().Trim() == dataUserRow["documentType"].ToString().ToUpper().Trim() &&
                    counterpartyContent.metadata.account_number.Trim() == dataUserRow["accountNumber"].ToString().Trim() &&
                    counterpartyContent.type.ToUpper().Trim() == dataUserRow["accountType"].ToString().ToUpper().Trim() &&
                    counterpartyContent.metadata.counterparty_email.ToUpper().Trim() == dataUserRow["email"].ToString().ToUpper().Trim() &&
                    counterpartyContent.metadata.counterparty_phone.Trim() == dataUserRow["phone"].ToString().Trim()
                   ;
        }
        private string GeterrorDictionary(string code)
        {
            Dictionary<string, string> _errorDictionary;

            // Ruta del archivo JSON
            var filePath = Path.Combine(_configuration["route:pathErrorsCobre"], "errorsCobre.json");

            // Lee el contenido del archivo y deserializa a un diccionario
            var jsonContent = System.IO.File.ReadAllText(filePath);
            _errorDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);

            // Verificar si el código existe y devolver el mensaje
            if (_errorDictionary.TryGetValue(code, out var message)) return message;

            // Si el código no se encuentra, devolver un mensaje predeterminado
            return "Código de error no encontrado";
        }

        private bool ISValidateAccountBalance(DataTable dataUser, JsonElement jsonElementWebHook)
        {
            // Verifica que "umbral" no sea null ni vacío
            if (dataUser.Rows[0]["umbral"] != DBNull.Value && int.TryParse(dataUser.Rows[0]["umbral"].ToString(), out int umbral))
            {
                umbral *= 100;

                // Accede a "obtained_balance" del JSON
                if (jsonElementWebHook.TryGetProperty("content", out JsonElement contentElement) &&
                    contentElement.TryGetProperty("source", out JsonElement sourceElement) &&
                    sourceElement.TryGetProperty("obtained_balance", out JsonElement obtainedBalanceElement))
                {
                    long obtainedBalance;

                    // Verifica si el elemento JSON es de tipo número
                    if (obtainedBalanceElement.ValueKind == JsonValueKind.Number &&
                        obtainedBalanceElement.TryGetInt64(out obtainedBalance))
                    {
                        // Valida si el umbral supera el saldo obtenido
                        if (umbral >= obtainedBalance)
                        {
                            SendInsufficientBalanceEmail(dataUser);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // Método auxiliar para enviar el correo
        private void SendInsufficientBalanceEmail(DataTable dataUser)
        {
            string clientName = dataUser.Rows[0]["NombreCLiente"].ToString();
            string clientEmail = dataUser.Rows[0]["EmailClient"].ToString();
            string msg = $"No se cuenta con el saldo suficiente en la plataforma de cobre V3 - Cliente: {clientName}";

            Utilities utilities = new(_configuration);
            utilities.SendEmail(clientEmail, "SALDO INSUFICIENTE COBRE", msg, false, "");
        }
    }
}