using QRCoder;
using SelectPdf;
using System.Data;
using System.Drawing;
using WebServicesAnticiposNomina.Models.Class;
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
                        if (AdvanceRequest.Base64Image.Length > 6)
                            utilities.SavePhoto(AdvanceRequest.Base64Image, int.Parse(dataUser.Rows[0]["id_anticipo"].ToString()));

                        if (AdvanceRequest.Email.Count() > 6)
                        {
                            string bodyMessage = utilities.GetBodyEmailCode(AdvanceRequest.Code, dataUser, 1);
                            utilities.SendEmail(AdvanceRequest.Email, "Código anticipo", bodyMessage, true, "");
                        }
                        else
                            utilities.SendSms(AdvanceRequest.CellPhone, $"Codigo para el anticipo es: {AdvanceRequest.Code}");

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
                        ApiCobreCore apiCobreCore = new(_configuration);
                        ResponseCobre responseCobre = apiCobreCore.PostPaymentAdvance(dataUser, Token);

                        responseModels.CodeResponse ="201";
                        responseModels.DataApiCobre = responseCobre;
                        responseModels.Token = Token;
                        string bodyMessage;
                        switch (responseCobre.code)
                        {
                            case "200":
                                //Pendiente por revision del administrador
                                bodyMessage = utilities.GetBodyEmailCode("", dataUser, 4);
                                utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo Pendiete", bodyMessage, true, "");
                                break;
                            case "201":  
                                advanceRequest.uuid = responseCobre.data;
                                advanceModel.PostAdvance(advanceRequest, 4);
                                //"Transaccion registrada"
                                bodyMessage = utilities.GetBodyEmailCode("", dataUser, 3);
                                utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo generado", bodyMessage, true, "");
                                break;
                            case "204":
                                //Faltas datos personales, llamar a la linea de atencion de JIRO.
                                bodyMessage = utilities.GetBodyEmailCode("", dataUser, 2);
                                utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Anticipo Rechazado", bodyMessage, true, "");
                                advanceModel.PostAdvance(advanceRequest, 5);
                                break;
                        }   
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
                        utilities.SendSms(dataUser.Rows[0]["celular"].ToString(), "Anticipo Aprovado");
                        responseModels = PostAdvance(advanceRequest, Token);
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