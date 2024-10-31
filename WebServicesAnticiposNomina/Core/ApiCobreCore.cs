using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;
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
                        responseModels.Message = "Error datos personales";
                        responseModels.code = "204";
                    }
                    else
                    {
                        // asignar id destinatario
                        dataUser.Rows[0]["id_cuenta_pasarela"] = id_cuenta_pasarela;

                        // Valance de la cuenta
                        int Balance = apiCobre.GetBalanceBank(TokenApi, dataUser);

                        if (Balance > 0)
                        {
                            // crear transacion 
                            var paymant = PutPaymentClass(dataUser);
                            if (paymant != null)
                            {
                                try
                                {
                                    responseModels = apiCobre.PostPayment(TokenApi, paymant, dataUser);
                                }
                                catch (Exception)
                                {
                                    LogsModel logsModel = new LogsModel(_configuration);
                                    LogRequest logRequest = new LogRequest()
                                    {
                                        Origen = "PostPayment - error",
                                        Request_json = paymant.noveltyDetails[0].beneficiary.name.Trim(), // Organizar credenciales de seccion 
                                        Observacion = "Eror al registrar la transaccion en cobre",
                                    };
                                    logsModel.PostLog(logRequest);
                                    throw;
                                }
                            }
                            else
                            {
                                responseModels.Message = "Faltan datos personales";
                                responseModels.code = "204";
                            }                            
                        }
                        else
                        {
                            string msg = "No tiene saldo la plataforma";
                            responseModels.Message = msg;
                            responseModels.code = "200";
                            _ = utilities.SendEmail(dataUser.Rows[0]["EmailClient"].ToString(), "SALDO INSUFICIENTE COBRE", msg, false, "");
                        }
                    }
                }
            }
            catch (Exception)
            {
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
        public string WeebHookPayment(TransactionRequest transactionRequest)
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
        public string? GetDataAccountUser(DataTable dataUser, string Token)
        {
            string? destination_id = null;
            string? dataAccountUser = dataUser.Rows[0]["id_cuenta_pasarela"].ToString();
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
                DataRow dataUserRow = dataUser.Rows[0];

                bool isFiel = IsCounterpartyFieldsMatching(counterpartyContent, dataUserRow);
                if (counterpartyContent == null || !isFiel)
                {
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
                    destination_id = this.GetDataAccountUser(dataUser, Token);
                }
            }
            return destination_id;
        }
        private bool IsCounterpartyFieldsMatching(CounterpartyContent counterpartyContent, DataRow dataUserRow)
        {
            return counterpartyContent.metadata.counterparty_id_number == dataUserRow["documentNumber"].ToString() &&
                   counterpartyContent.metadata.counterparty_id_type.ToUpper() == dataUserRow["documentType"].ToString().ToUpper() &&
                   counterpartyContent.metadata.account_number == dataUserRow["accountNumber"].ToString() &&
                   counterpartyContent.type.ToUpper() == dataUserRow["accountType"].ToString().ToUpper() &&
                   counterpartyContent.metadata.counterparty_email.ToUpper() == dataUserRow["email"].ToString().ToUpper() &&
                   counterpartyContent.metadata.counterparty_phone == dataUserRow["phone"].ToString();
        }
    }
}