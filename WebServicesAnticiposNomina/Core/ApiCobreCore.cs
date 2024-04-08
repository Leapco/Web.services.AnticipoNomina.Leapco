using MimeKit.Encodings;
using QRCoder;
using System.Data;
using System.Drawing;
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

        public ResponseCobre PostPaymentAdvance(DataTable dataUser, string Token)
        {
            ResponseCobre responseModels = new();
            try
            {
                responseModels.Message = "Token expirado";
                responseModels.code = "401";

                SecurityCore securityCore = new(_configuration);
                ApiCobre apiCobre = new(_configuration);

                if (securityCore.IsTokenValid(Token))
                {
                    Utilities utilities = new(_configuration);
                    string TokenApi = apiCobre.PostAuthToken(Token);
                    if (TokenApi != "false")
                    {
                        int Balance = apiCobre.GetBalanceBank(TokenApi);

                        if (Balance > 0)
                        {
                            var paymant = PutPaymentClass(dataUser);
                            if (paymant != null)
                            {
                                responseModels = apiCobre.PostPayment(TokenApi, paymant);
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
                            utilities.SendSms("3007185717", msg);
                        }
                    }
                    else
                    {
                       utilities.SendSms("3007185717", "Error al crear token de cobre");
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
        public PaymentClass PutPaymentClass(DataTable dataUser)
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
                    var email =  utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo consignado", bodyEmail, true,
                                    _configuration["route:pathContrato"] + $"\\{dataUser.Rows[0]["id_anticipo"]}.pdf");
                   return "201";
                }
                else
                {
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
    }
}
