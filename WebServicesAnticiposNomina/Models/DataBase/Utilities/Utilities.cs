using Azure.Core;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using WebServicesAnticiposNomina.Models.Class.Request;

namespace WebServicesAnticiposNomina.Models.DataBase.Utilities
{
    public class Utilities
    {
        private readonly IConfiguration _configuration;

        public Utilities(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string RetornarSetting()
        {
            return _configuration.GetConnectionString("ConnectionSQL");
        }
        public string GetSHA256(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                return Convert.ToBase64String(hashBytes);
            }
        }
        public async Task SendSms(string? celular, string? message)
        {
            celular = "57" + celular;

            var post = new Dictionary<string, object>
            {
                { "to", new[] { celular } },
                { "message", message },
                { "from", "msg" },
                { "campaignName", "GIGHA" }
            };

            string? user = _configuration["Sms:user"];
            string? password = _configuration["Sms:password"];

            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}")));

                    var content = new StringContent(JsonConvert.SerializeObject(post), Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync("https://dashboard.360nrs.com/api/rest/sms", content);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        public string GenerarCodigo()
        {
            Random random = new Random();
            int codigoNumerico = random.Next(100000, 999999);
            return codigoNumerico.ToString();
        }       
        public async Task<bool> SendEmail(string toAddress, string subject, string body, bool IsBodyHtml, string? pdfFilePath)
        {
            bool result = false;
            try
            {
                string? _smtpServer = _configuration["Email:smtpServer"];
                int _smtpPort = int.Parse(_configuration["Email:smtpPort"]);
                string? _smtpUsername = _configuration["Email:smtpUsername"];
                string? _smtpPassword = _configuration["Email:smtpPassword"];                

                //toAddress = "informatica3@gigha.com.co";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_smtpUsername);
                    mail.To.Add(toAddress);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = IsBodyHtml; // Si el cuerpo del correo contiene HTML

                    // Adjuntar el archivo PDF
                    if (!string.IsNullOrEmpty(pdfFilePath) && pdfFilePath.Count() > 10)
                    {                   
                        Attachment attachment = new(pdfFilePath, MediaTypeNames.Application.Pdf);
                        mail.Attachments.Add(attachment);                    
                    }

                    using (SmtpClient smtpClient = new(_smtpServer, _smtpPort))
                    {
                        try
                        {
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                            smtpClient.EnableSsl = true;
                            smtpClient.Timeout = 90000; // 20 segundos

                            smtpClient.Send(mail);
                            result = true;
                        }
                        catch (Exception ex)
                        {
                            await SendSms("3007185717", ex.Message);
                            result = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public string SavePhoto(string Base64Image, int id_anticipo)
        {
            // Decodificar la imagen base64
            byte[] imageBytes = Convert.FromBase64String(Base64Image);

            // Generar un nombre de archivo único
            string fileName = $"{id_anticipo}.jpg";
            string pathPhotoAdvance = _configuration["route:pathPhotoAdvance"];

            // Ruta donde se guardará la imagen
            string imagePath = Path.Combine(pathPhotoAdvance, "", fileName);

            // Guardar la imagen en el servidor
            System.IO.File.WriteAllBytes(imagePath, imageBytes);

            // Devolver la ruta de la imagen guardada
            string imageUrl = pathPhotoAdvance+ $"\\{fileName}";

            return imageUrl;
        }
        public string GetBodyEmailCode(string Code, DataTable dataUser)
        {
            string body = string.Empty;
            try
            {
                string? color_primario = dataUser.Rows[0]["color_primario"].ToString();
                string? color_secundario = dataUser.Rows[0]["color_secundario"].ToString();
                string? color_terciario = dataUser.Rows[0]["color_terciario"].ToString();
                string? logo = dataUser.Rows[0]["logo"].ToString();

                body = $"<!DOCTYPE html>\r\n<html lang='en'>\r\n<head>\r\n    <meta charset='UTF-8'>\r\n    <meta name='viewport' content='width=device-width, initial-scale=1.0'>\r\n    <title>Código de verificación</title>\r\n    <link rel='preconnect' href='https://fonts.googleapis.com'>\r\n    <link\r\n        rel='preconnect'\r\n        href='https://fonts.gstatic.com'\r\n        crossorigin='crossorigin'>\r\n    <link\r\n        href='https://fonts.googleapis.com/css2?family=Inter:wght@100..900&display=swap'\r\n        rel='stylesheet'>\r\n    <style>\r\n        html {{\r\n            height: fit-content;\r\n            background-color: #f6f6f6;\r\n        }}\r\n        body {{\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            background-color: {color_primario};  /*Color primario */\r\n            font-family: 'Inter', sans-serif;\r\n        }}\r\n        header {{\r\n            display: flex;\r\n            align-items: center;\r\n            justify-content: space-between;\r\n            padding: 8px 64px;\r\n        }}\r\n        .header__logo {{\r\n            width: 112px;\r\n        }}\r\n        .header > span {{\r\n            font-weight: bold;\r\n            color: white; /* Color titulo */\r\n        }}\r\n        .main-content {{\r\n            display: flex;\r\n            flex-direction: column;\r\n            align-items: center;\r\n            padding: 24px 64px;\r\n        }}\r\n        .main-content__code {{\r\n            padding: 14px 24px;\r\n            border-radius: 4px;\r\n            background-color: {color_secundario}; /* Color secundario */\r\n            margin-bottom: 16px;\r\n            font-weight: bold;\r\n        }}\r\n        .main-content__code > p {{\r\n            color: white; /* Color titulo */\r\n            margin: 0;\r\n        }}\r\n        .main-content__title {{\r\n            margin: 0;\r\n            font-size: 24px;\r\n            font-weight: 700;\r\n            color: white; /* Color titulo */\r\n        }}\r\n        .document-section {{\r\n            display: flex;\r\n            flex-direction: column;\r\n            align-items: center;\r\n            text-align: center;\r\n            padding: 24px 64px;\r\n            background-color: white;\r\n        }}\r\n        .document-section__text {{\r\n            color: black; /* Color texto */\r\n            margin: 0;\r\n        }}\r\n        .document-section__button {{\r\n            margin-top: 16px;\r\n            font-size: 16px;\r\n            padding: 14px 24px;\r\n            border-radius: 4px;\r\n            border: none;\r\n            background-color: {color_secundario}; /* Color secundario */\r\n            color: white; /* Color titulo */\r\n        }}\r\n        .footer {{\r\n            padding: 24px 64px;\r\n            color: black; /* Color texto */\r\n            text-align: center;\r\n            background-color: {color_terciario}; /* Color terciario */\r\n            font-size: 12px;\r\n        }}\r\n        .footer > span:nth-child(1) {{\r\n            display: block;\r\n            font-weight: 600;\r\n            width: 100%;\r\n            font-size: 14px;\r\n        }}\r\n        .footer > span:nth-child(2) {{\r\n            display: block;\r\n            margin-bottom: 16px;\r\n        }}\r\n        .document-section__button {{\r\n            cursor: pointer;\r\n        }}\r\n    </style>\r\n</head>\r\n<body>\r\n    <header class='header'>\r\n        <img\r\n            src='https://sigha.com.co/anticipos/_lib/file/img/{logo}'\r\n            alt='logo_gigha'\r\n            class='header__logo'>\r\n        <span>Anticipos de Nómina</span>\r\n    </header>\r\n    <div class='main-content'>\r\n        <div class='main-content__code'>\r\n            <p>{Code}</p>\r\n        </div>\r\n        <h1 class='main-content__title'>Código de verificación</h1>\r\n    </div>\r\n    <div class='document-section'>\r\n        <p class='document-section__text'>Con este código puedes verificar tu identidad en Anticipos de Nómina. Si no solicitaste el código has click en el siguiente botón.</p>\r\n            <button class='document-section__button'>No he solicitado ningún código</button>\r\n        </a>\r\n    </div>\r\n    <div class='footer'>\r\n        <span class='footer__info'>Esta es una cuenta automática para envío de información.</span>\r\n        <span class='footer__info'>Por favor NO responda este correo ni escriba a esta dirección.</span>\r\n        <span class='footer__info'>Si tiene alguna duda o inquietud, por favor comuníquese al\r\n            <span class='footer__contact'>444 76 00 ext 1061 o 1062</span>\r\n            <span class='footer__contact'></span>\r\n            o escriba al correo\r\n            <a href='mailto:facturacion@gigha.com.co' class='footer__contact'>facturacion@gigha.com.co</a>\r\n        </span>\r\n    </div>\r\n</body>\r\n</html>";
            }
            catch (Exception)
            {
                body = $"Codigo de verificacion: {Code}";
            }
            return body;
        }
    }
}