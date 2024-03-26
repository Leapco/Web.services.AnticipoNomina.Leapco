using QRCoder;
using SelectPdf;
using System.Data;
using System.Drawing;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;
using WebServicesAnticiposNomina.Models.DataBase;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;

namespace WebServicesAnticiposNomina.Core
{
    public class AdvanceCore
    {
        private readonly IConfiguration _configuration;

        public AdvanceCore(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ResponseModels PostCodeAdvance(AdvanceRequest AdvanceRequest, string Token)
        {
            ResponseModels responseModels = new();
            try
            {
                SecurityCore securityCore = new(_configuration);
                if (securityCore.IsTokenValid(Token))
                {
                    AdvanceModel advanceModel = new(_configuration);
                    Utilities utilities = new(_configuration);
                    AdvanceRequest.Code = utilities.GenerarCodigo();
                    DataTable dataUser = advanceModel.PostAdvance(AdvanceRequest, 1);
                    responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                    if (dataUser.Rows[0]["state"].ToString() == "1")
                    {
                        if (AdvanceRequest.Base64Image.Length > 0)
                            utilities.SavePhoto(AdvanceRequest.Base64Image, int.Parse(dataUser.Rows[0]["id_anticipo"].ToString()));

                        string bodyMessage = $"Codigo de verificacion: {AdvanceRequest.Code}";

                        if (AdvanceRequest.Email.Count() > 5)
                        {
                            bodyMessage = utilities.GetBodyEmailCode(AdvanceRequest.Code, dataUser);
                            utilities.SendEmail(AdvanceRequest.Email, "Código anticipo", bodyMessage, true, "");
                        }
                        else
                            utilities.SendSms(AdvanceRequest.CellPhone, bodyMessage);

                        responseModels.Token = Token;
                        responseModels.CodeResponse = "201";
                        responseModels.Data = "{'codigo': '" + AdvanceRequest.Code + "', 'Email': '" + dataUser.Rows[0]["email"].ToString() + "'}";
                    }
                    else
                        responseModels.CodeResponse = "200";
                }
                else
                {
                    responseModels.MessageResponse = "Token expirado";
                    responseModels.CodeResponse = "401";
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels PostAdvance(AdvanceRequest advanceRequest, string Token)
        {
            ResponseModels responseModels = new();
            try
            {
                SecurityCore securityCore = new(_configuration);
                if (securityCore.IsTokenValid(Token))
                {
                    AdvanceModel advanceModel = new(_configuration);
                    Utilities utilities = new(_configuration);
                    DataTable dataUser = advanceModel.PostAdvance(advanceRequest, 2);
                    responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                    if (dataUser.Rows[0]["state"].ToString() == "1")
                    {


                        if (!CreateContract(dataUser)) CreateContract(dataUser);

                        string bodyEmail = GetBodyEmailCode(dataUser);
                        utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo generado", bodyEmail, true, _configuration["route:pathContrato"] + $"\\{dataUser.Rows[0]["id_anticipo"]}.pdf");

                        responseModels.Token = Token;
                        responseModels.CodeResponse = "201";
                    }
                    else
                        responseModels.CodeResponse = "200";
                }
                else
                {
                    responseModels.MessageResponse = "Token expirado";
                    responseModels.CodeResponse = "401";
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels PutStatudAdvance(AdvanceRequest advanceRequest, string Token)
        {
            ResponseModels responseModels = new();
            try
            {
                SecurityCore securityCore = new(_configuration);
                if (Token == _configuration["JwtSettings:SecretKeyAdmin"])
                {
                    AdvanceModel advanceModel = new(_configuration);
                    Utilities utilities = new(_configuration);
                    DataTable dataUser = advanceModel.PostAdvance(advanceRequest, 3);
                    responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                    if (dataUser.Rows[0]["state"].ToString() == "1")
                    {
                        // falta envio a cobre una vez se apruebe

                        utilities.SendSms(dataUser.Rows[0]["celular"].ToString(), "Anticipo Aprovado");
                        responseModels.CodeResponse = "201";
                    }
                    else
                        responseModels.CodeResponse = "200";
                }
                else
                {
                    responseModels.MessageResponse = "Token expirado";
                    responseModels.CodeResponse = "401";
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public bool CreateContract(DataTable dataTable)
        {
            string pathContract = _configuration["route:pathTemplace"] + $"\\{dataTable.Rows[0]["id_anticipo"]}.html";
            try
            {
                if (File.Exists(pathContract)) File.Delete(pathContract);

                // creo el contrato base en html
                File.Copy(_configuration["route:pathTemplace"] + "\\Contract.html", pathContract);

                // Leer la imagen y la convierto en base64
                string pathImagenClient = _configuration["route:pathPhotoAdvance"] + "\\" + dataTable.Rows[0]["id_anticipo"] + ".jpg";
                byte[] imageBytes = System.IO.File.ReadAllBytes(pathImagenClient);
                string base64Image = Convert.ToBase64String(imageBytes);

                // region
                string base64Signature = GenerateQRCode(dataTable.Rows[0]["firma_digital"].ToString());
                //agregar al contrato

                // modifico el contrato en html
                this.GetHtmlContent(pathContract, dataTable.Rows[0]["Contrato"].ToString(), base64Image, base64Signature);

                // Convertir HTML a PDF
                var htmlCode = File.ReadAllText(pathContract);
                HtmlToPdf converter = new();
                PdfDocument doc = converter.ConvertHtmlString(htmlCode);
                byte[] data = doc.Save();
                doc.Close();

                string pathContractPdf = _configuration["route:pathContrato"] + $"\\{dataTable.Rows[0]["id_anticipo"]}.pdf";
                if (File.Exists(pathContractPdf)) File.Delete(pathContractPdf);
                // Guardar los bytes en un archivo PDF en la ruta especificada
                File.WriteAllBytes(pathContractPdf, data);

                // Elimino foto y contrato en html
                //File.Delete(pathContract);
                //File.Delete(pathImagenClient);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public bool GetHtmlContent(string pathContract, string? textContract, string base64Image, string base64Signature)
        {
            try
            {
                string htmlContent = System.IO.File.ReadAllText(pathContract);
                htmlContent = htmlContent.Replace("<p id=\"contrato\"></p>", $"<p id=\"textContract\">{textContract}</p>");
                System.IO.File.WriteAllText(pathContract, htmlContent);

                htmlContent = htmlContent.Replace("<img class=\"img-item\" id=\"foto\">", $"<img class=\"img-item\" id=\"foto\" src=\"data:image/png;base64,{base64Image}\" alt =\"Imagen Base64\">");
                System.IO.File.WriteAllText(pathContract, htmlContent);

                htmlContent = htmlContent.Replace("<img class=\"img-item\" id=\"QR\">", $"<img class=\"img-item\" id=\"QR\" src=\"data:image/png;base64,{base64Signature}\" alt =\"Imagen Base64\" width=\"50\" height=\"50\">");
                System.IO.File.WriteAllText(pathContract, htmlContent);

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetBodyEmailCode(DataTable dataUser)
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
        public string GenerateQRCode(string? text)
        {
            // generador de códigos QR
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                // Utiliza el código QR para generar una imagen bitmap
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    // Obtiene la imagen del código QR con un tamaño de 20x20
                    Bitmap qrCodeImage = qrCode.GetGraphic(20);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        byte[] imageBytes = stream.ToArray();
                        qrCodeImage.Dispose();
                        qrCode.Dispose();
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
        }
    }
}