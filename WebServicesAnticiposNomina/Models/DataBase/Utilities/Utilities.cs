using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
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
        public (string Salt, string Hash) EncryptWithSalt(string input)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            string salt = Convert.ToBase64String(saltBytes);
            string saltedInput = input + salt;

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedInput));
                string hash = Convert.ToBase64String(hashBytes);
                return (salt, hash);
            }
        }

        public static bool VerifyWithSalt(string input, string salt, string hash)
        {
            string saltedInput = input + salt;

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedInput));
                string newHash = Convert.ToBase64String(hashBytes);
                return newHash == hash;
            }
        }
        public async Task SendSms(string? celular, string message)
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
                        LogsModel logsModel = new LogsModel(_configuration);
                        LogRequest logRequest = new LogRequest()
                        {
                            Origen = "SendSms",
                            Request_json = response.ToString(),
                            Observacion = "Revisar envio de correo"
                        };
                        logsModel.PostLog(logRequest);

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
        public async Task<bool> SendEmail(string? toAddress, string subject, string body, bool IsBodyHtml, string? pdfFilePath)
        {
            LogsModel logsModel = new LogsModel(_configuration);
            bool result = false;
            try
            {
                string? _smtpServer = _configuration["Email:smtpServer"];
                int _smtpPort = int.Parse(_configuration["Email:smtpPort"]);
                string? _smtpUsername = _configuration["Email:smtpUsername"];
                string? _smtpPassword = _configuration["Email:smtpPassword"];

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
                            LogRequest logRequest = new LogRequest()
                            {
                                Origen = "SendEmail",
                                Request_json = $"Email = {toAddress}",
                                Observacion = "No se envio en correo electronico"
                            };
                            logsModel.PostLog(logRequest);
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
            string imageUrl = pathPhotoAdvance + $"\\{fileName}";

            return imageUrl;
        }
        public string GetBodyEmailCode(string Code, DataTable dataUser, int? option)
        {
            string body = string.Empty;
            try
            {
                string? color_primario = dataUser.Rows[0]["color_primario"].ToString();
                string? color_secundario = dataUser.Rows[0]["color_secundario"].ToString();
                string? color_terciario = dataUser.Rows[0]["color_terciario"].ToString();
                string? logo = dataUser.Rows[0]["logo"].ToString();
                string icono = "";
                string accion = "";
                string texto = "";

                switch (option)
                {
                    case 1:
                        body = $"<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Código de verificación</title>\r\n    <link rel='preconnect' href='https://fonts.googleapis.com'>\r\n    <link\r\n        rel='preconnect'\r\n        href='https://fonts.gstatic.com'\r\n        crossorigin='crossorigin'>\r\n    <link\r\n        href='https://fonts.googleapis.com/css2?family=Inter:wght@100..900&display=swap'\r\n        rel='stylesheet'>\r\n    <style>\r\n        html {{\r\n            height: fit-content;\r\n            background-color: #f6f6f6;\r\n        }}\r\n        body {{\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            background-color: white;\r\n            font-family: 'Inter', sans-serif;\r\n        }}\r\n        .header {{\r\n            display: flex;\r\n            align-items: center;\r\n            justify-content: space-between;\r\n            padding: 8px 64px;\r\n            background-color: {color_primario}; /* Color primario */\r\n        }}\r\n        .header__logo {{\r\n            max-width: 112px;\r\n        }}\r\n        .main-content {{\r\n            display: flex;\r\n            flex-direction: column;\r\n            align-items: center;\r\n            padding: 24px 64px;\r\n            text-align: center;\r\n        }}\r\n        .main-content__code {{\r\n            padding: 14px 24px;\r\n            border-radius: 4px;\r\n            font-size: 16px;\r\n            background-color: {color_primario}; /* Color primario */\r\n            font-weight: 600;\r\n        }}\r\n        .main-content__code > p {{\r\n            color: white; /* Color titulo */\r\n            margin: 0;\r\n        }}\r\n        .main-content__title {{\r\n            margin: 0;\r\n            margin-bottom: 1rem;\r\n            font-size: 20px;\r\n            font-weight: 700;\r\n            color: {color_primario}; /* Color primario */\r\n        }}\r\n        .main-content__text {{text-align: center;\r\n            margin: 0;\r\n            margin-bottom: 1rem;\r\n            font-size: 14px;\r\n        }}\r\n        .footer {{\r\n            padding: 24px 64px;\r\n            color: black; /* Color texto */\r\n            text-align: center;\r\n            background-color: #e7e7e7;\r\n            font-size: 12px;\r\n        }}\r\n        .footer > .footer-texto {{\r\n            display: block;\r\n            font-weight: 400;\r\n            width: 100%;\r\n            font-size: 14px;\r\n        }}\r\n    </style>\r\n</head>\r\n<body>\r\n    <div class='header'>       \r\n        <img\r\n        src='https://sigha.com.co/sigha/_lib/img/logos/Anticipos_Nomina_White.png'\r\n        alt='logo_gigha'\r\n        >\r\n        <img\r\n            src='https://sigha.com.co/anticipos/_lib/file/img/{logo}'\r\n            alt='logo_gigha'\r\n            class=\"header__logo\"\r\n        > \r\n    </div>\r\n    <div class='main-content'>\r\n        <h1 class='main-content__title'>Código de verificación</h1>\r\n        <p class='main-content__text'>Con este código podras verificar tu identidad en Anticipos de Nómina.</p>\r\n        <div class=\"main-content__code\">\r\n            <p>{Code}</p>\r\n        </div>\r\n    </div>\r\n    <div class='footer'>\r\n        <span class=\"footer-texto\">Esta es una cuenta automática para envío de información. Por favor NO responda este correo ni escriba a esta dirección.</span>\r\n    </div>\r\n</body>\r\n</html>";
                        break;
                    case 2:
                        icono = "xmark";
                        accion = "rechazado";
                        texto = $"Tu anticipo fue {accion} por el sistema o tu empleador.";
                        break;
                    case 3:
                        icono = "check";
                        accion = "generado";
                        texto = $"Tu anticipo ha sido generado por el sistema o aprobado por tu empleador. En unos instantes estarás recibiendo la consignación de tu anticipo junto a su comprobante.";
                        break;
                    case 4:
                        icono = "exclamation";
                        accion = "pendiente";
                        texto = $"Tu anticipo fue {accion} por el sistema o tu empleador.";
                        break;
                    case 5:
                        accion = "consignado";
                        icono = "doc";
                        texto = "Tu anticipo ha sido consignado, aqui tienes el comprobante de la transacción.";
                        break;
                }

                if (option != 1)
                    body = $"<!DOCTYPE html>\r\n<html lang='en'>\r\n    <head>\r\n        <meta charset='UTF-8'>\r\n        <meta name='viewport' content='width=device-width, initial-scale=1.0'>\r\n        <title>Contrato de Anticipos</title>\r\n        <link rel='preconnect' href='https://fonts.googleapis.com'>\r\n        <link\r\n            rel='preconnect'\r\n            href='https://fonts.gstatic.com'\r\n            crossorigin='crossorigin'>\r\n        <link\r\n            href='https://fonts.googleapis.com/css2?family=Inter:wght@100..900&display=swap'\r\n            rel='stylesheet'>\r\n            <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css\" integrity=\"sha512-DTOQO9RWCH3ppGqcWaEA1BIZOC6xxalwEsw9c2QQeAIftl+Vegovlnee1c9QX4TctnWMn13TZye+giMm8e2LwA==\" crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\" />\r\n        <style>\r\n            html {{\r\n                height: fit-content;\r\n                background-color: #f6f6f6;\r\n            }}\r\n            #body {{\r\n                max-width: 600px;\r\n                margin: 0 auto;\r\n                background-color: white;\r\n                font-family: 'Inter', sans-serif;\r\n            }}\r\n            #header {{\r\n                padding: 8px 64px;\r\n                display: flex;\r\n                justify-content: space-between;\r\n                align-items: center;\r\n                background-color: {color_primario}; /* Color primario */\r\n            }}\r\n            .header__logo-empresa {{\r\n                width: 96px;\r\n            }}\r\n            .main-content {{\r\n                display: flex;\r\n                flex-direction: column;\r\n                align-items: center;\r\n                padding: 24px 64px;\r\n            }}\r\n            .main-content__logo {{\r\n                display: flex;\r\n                align-items: center;\r\n                justify-content: center;\r\n                background-color: {color_primario}; /* Color primario */\r\n                color: white; /* Color titulo */\r\n                width: 112px;\r\n                height: 112px;\r\n                border-radius: 100%;\r\n            }}\r\n            .main-content__title {{\r\n                margin: 0;\r\n                margin-bottom: 1rem;\r\n                font-size: 20px;\r\n                font-weight: 700;\r\n                color: {color_primario}; /* Color primario */\r\n            }}\r\n            .main-content__text {{text-align: center;\r\n                margin: 0;\r\n                font-size: 14px;\r\n                margin-bottom: 1rem;\r\n \r\n            }}\r\n            .footer {{\r\n                padding: 24px 64px;\r\n                color: black; /* Color texto */\r\n                text-align: center;\r\n                background-color: #e7e7e7;\r\n                font-size: 14px;\r\n            }}\r\n        </style>\r\n    </head>\r\n    <body id=\"body\">\r\n        <div id='header'>\r\n            <img\r\n                src=\"https://sigha.com.co/sigha/_lib/img/logos/Anticipos_Nomina_White.png\"\r\n                alt='logo_anticipos_nomina'\r\n                class='header__logo-anticipos'>\r\n            <img\r\n                src='https://sigha.com.co/anticipos/_lib/file/img/{logo}'\r\n                alt='logo_gigha'\r\n                class='header__logo-empresa'>\r\n        </div>\r\n        <div class='main-content'>\r\n            <h1 class='main-content__title'>Anticipo {accion}</h1>\r\n            <p class='main-content__text'>\r\n                {texto}    \r\n            </p>\r\n            <div class=\"main-content__logo\">\r\n                <img src=\"https://sigha.com.co/anticipos/_lib/img/file-circle-{icono}-solid.svg\" alt=\"Anticipo\">\r\n            </div>\r\n        </div>\r\n        <div class='footer'>\r\n            <span class=\"footer-texto\">Esta es una cuenta automática para envío de información. Por favor NO responda este correo ni escriba a esta dirección.</span>\r\n        </div>\r\n    </body>\r\n</html>";

            }
            catch (Exception)
            {
                body = $"Codigo de verificacion: {Code}";
            }
            return body;
        }
    }
}