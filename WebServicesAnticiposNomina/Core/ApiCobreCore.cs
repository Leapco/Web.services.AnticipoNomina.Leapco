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
            advanceRequest.uuid = transactionRequest.NoveltyDetailUuid;

            try
            {
                if (transactionRequest.Status == "FINISHED")
                {
                    DataTable dataUser = advanceModel.PostAdvance(advanceRequest, 6);

                   if (!advanceCore.CreateContract(dataUser)) advanceCore.CreateContract(dataUser);

                   string bodyEmail = this.GetBodyContract(dataUser);

                   var email =  utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo generado", bodyEmail, true,
                                    _configuration["route:pathContrato"] + $"\\{dataUser.Rows[0]["id_anticipo"]}.pdf");
                   return "201";
                }
                else
                {
                    DataTable dataUser = advanceModel.PostAdvance(advanceRequest, 7);
                    //consultar el por que del rechazo
                    string code = "200";
                    if (dataUser.Rows[0]["state"].ToString() == "1")
                    {
                        utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo generado", "Rechazado desde el banco", false, "");
                        code = "201";
                    }

                    return code;
                }
            }
            catch (Exception ex)
            {
                return "500 " + ex.Message;
            }
        }
        public string GetBodyContract(DataTable dataUser)
        {
            string body = string.Empty;
            try
            {
                string? color_primario = dataUser.Rows[0]["color_primario"].ToString();
                string? color_secundario = dataUser.Rows[0]["color_secundario"].ToString();
                string? color_terciario = dataUser.Rows[0]["color_terciario"].ToString();
                string? logo = dataUser.Rows[0]["logo"].ToString();

                body = $"<!DOCTYPE html>\r\n<html lang='en'>\r\n    <head>\r\n        <meta charset='UTF-8'>\r\n        <meta name='viewport' content='width=device-width, initial-scale=1.0'>\r\n        <title>Soporte de movimientos</title>\r\n        <link rel='preconnect' href='https://fonts.googleapis.com'>\r\n        <link\r\n            rel='preconnect'\r\n            href='https://fonts.gstatic.com'\r\n            crossorigin='crossorigin'>\r\n        <link\r\n            href='https://fonts.googleapis.com/css2?family=Inter:wght@100..900&display=swap'\r\n            rel='stylesheet'>\r\n        <style>\r\n            html {{\r\n                height: fit-content;\r\n                background-color: #f6f6f6;\r\n            }}\r\n            body {{\r\n                max-width: 600px;\r\n                margin: 0 auto;\r\n                background-color: #0d343d;\r\n                font-family: 'Inter', sans-serif;\r\n            }}\r\n            header {{\r\n                padding: 8px 64px;\r\n                display: flex;\r\n                justify-content: space-between;\r\n                align-items: center;\r\n            }}\r\n            .header__logo-empresa {{\r\n                width: 96px;\r\n            }}\r\n            .main-content {{\r\n                display: flex;\r\n                flex-direction: column;\r\n                align-items: center;\r\n                padding: 24px 64px;\r\n            }}\r\n            .main-content__logo {{\r\n                width: 112px;\r\n                margin-bottom: 16px;\r\n            }}\r\n            .main-content__title {{\r\n                font-size: 24px;\r\n                font-weight: 700;\r\n                color: white;\r\n            }}\r\n            .document-section {{\r\n                display: flex;\r\n                flex-direction: column;\r\n                align-items: center;\r\n                text-align: center;\r\n                padding: 24px 64px;\r\n                background-color: white;\r\n            }}\r\n            .document-section__text {{\r\n                color: #0d343d;\r\n            }}\r\n            .document-section__button {{\r\n                margin-top: 16px;\r\n                font-size: 16px;\r\n                padding: 14px 24px;\r\n                border-radius: 4px;\r\n                border: none;\r\n                background-color: #0d343d;\r\n                color: white;\r\n            }}\r\n            .footer {{\r\n                padding: 24px 64px;\r\n                color: #0d343d;\r\n                text-align: center;\r\n                background-color: #e7e7e7;\r\n                font-size: 12px;\r\n            }}\r\n            .footer > span:nth-child(1) {{\r\n                display: block;\r\n                font-weight: 600;\r\n                width: 100%;\r\n                font-size: 14px;\r\n            }}\r\n            .footer > span:nth-child(2) {{\r\n                display: block;\r\n                margin-bottom: 16px;\r\n            }}\r\n            .document-section__button {{\r\n                cursor: pointer;\r\n            }}\r\n        </style>\r\n    </head>\r\n    <body>\r\n        <header class='header'>\r\n            <img\r\n                src=\"https://sigha.com.co/sigha/_lib/img/logos/Anticipos_Nomina_White.png\"\r\n                alt='logo_anticipos_nomina'\r\n                class='header__logo-anticipos'>\r\n            <img\r\n                src='https://sigha.com.co/se_ogh/_lib/img/sys__NM__img__NM__Logo_Gigha_2.png'\r\n                alt='logo_gigha'\r\n                class='header__logo-empresa'>\r\n        </header>\r\n        <div class='main-content'>\r\n            <img\r\n                src='https://sigha.com.co/sigha/_lib/img/mail-icon-file.png'\r\n                alt='logo_movimientos'\r\n                class='main-content__logo'>\r\n            <h1 class='main-content__title'>Contrato de Anticipo.</h1>\r\n        </div>\r\n        <div class='document-section'>\r\n            <p class='document-section__text'>\r\n                El contrato de tu anticipo está listo, hemos adjuntado el archivo en este correo, descárgalo y accede a la información de tu anticipo para corroborar o utilizar el documento como comprobante.            </p>\r\n        </div>\r\n        <div class='footer'>\r\n            <span class='footer__info'>Esta es una cuenta automática para envío de información.</span>\r\n            <span class='footer__info'>Por favor NO responda este correo ni escriba a esta dirección.</span>\r\n            <span class='footer__info'>Si tiene alguna duda o inquietud, por favor comuníquese al\r\n                <span class='footer__contact'>444 76 00 ext 1061 o 1062</span>\r\n                <span class='footer__contact'></span>\r\n                o escriba al correo\r\n                <a href='mailto:facturacion@gigha.com.co' class='footer__contact'>facturacion@gigha.com.co</a>\r\n            </span>\r\n        </div>\r\n    </body>\r\n</html>";
            }
            catch (Exception)
            {
                return "";
            }
            return body;
        }
    }
}
