using Microsoft.AspNetCore.Mvc;
using MimeKit;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.Class.Response;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;
using WebServicesAnticiposNomina.Models.PaymentGateway;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServicesAnticiposNomina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PruebaController : ControllerBase
    {
        public IConfiguration _Configuration;
        public PruebaController(IConfiguration configuration)
        {

            _Configuration = configuration;

        }
        //GET api/<PruebaController>/5
        //[HttpGet]
        //public async Task<string> Get(string celular)
        //{
        //    Utilities utilities = new(_Configuration);
        //    //await utilities.SendSms(celular, "Mensaje de prueba...");
        //    // utilities.SendEmail("informatica3@gigha.com.co", "Anticipo generado", "prueba", true, "");

        //    var message = new MimeMessage();
        //    message.From.Add(new MailboxAddress("joshua", "notificaciones@info.anticipodenomina.com.co"));
        //    message.To.Add(new MailboxAddress("", "informatica3@gigha.com.co"));
        //    message.Subject = "Prueba email";

        //    using (var client = new SmtpClient())
        //    {
        //        await client.ConnectAsync("mail.info.anticipodenomina.com.co", 465, false);
        //        await client.AuthenticateAsync("notificaciones@info.anticipodenomina.com.co", "infoadn2024$%");
        //        await client.SendAsync(message);
        //        await client.DisconnectAsync(true);
        //    }


        //    return "Mensaje enviado";
        //}
        // GET api/<PruebaController>/5
        //[HttpGet]
        //public async Task<string> GetSendMail(string correo, string asunto, string body)
        //{
        //    Utilities utilities = new(_Configuration);
        //    utilities.SendEmail(correo, asunto, body);    
        //    return "Email enviado";
        //}
        //public IActionResult CreateDocument(string imagePath)
        //{
        //    var docPath = @"C:\\Users\\uinformatica6.GIGHA\\OneDrive - GIGHA SAS - JIRO SAS\\Documentos\\Dev\\archive\\contratostemplateContract.docx";
        //    var pdfPath = @"C:\\Users\\uinformatica6.GIGHA\\OneDrive - GIGHA SAS - JIRO SAS\\Documentos\\Dev\\archive\\contratostemplateContract.pdf";

        //    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(docPath, WordprocessingDocumentType.Document))
        //    {
        //        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
        //        mainPart.Document = new Document();
        //        Body body = mainPart.Document.AppendChild(new Body());

        //        // Agregar párrafo con imagen
        //        Paragraph para = body.AppendChild(new Paragraph());
        //        Run run = para.AppendChild(new Run());
        //        Drawing drawing = run.AppendChild(new Drawing());
        //        Inline inline = drawing.AppendChild(new Inline());
        //        Extent extent = new Extent() { Cx = 990000L, Cy = 792000L };
        //        inline.AppendChild(extent);
        //        DocProperties docProperties = new DocProperties() { Id = (UInt32Value)1U, Name = "Picture 1" };
        //        inline.AppendChild(docProperties);
        //        Graphic graphic = inline.AppendChild(new Graphic());
        //        graphic.AppendChild(new GraphicData(new Picture(new BlipFill(new Blip() { Embed = imageId }, new Stretch(new FillRectangle())))) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" });
        //        wordDocument.MainDocumentPart.Document.Save();
        //    }

        //    // Convertir a PDF
        //    using (PdfDocument pdfDocument = PdfReader.Open(docPath, PdfDocumentOpenMode.Modify))
        //    {
        //        pdfDocument.Save(pdfPath);
        //    }

        //    return File(System.IO.File.ReadAllBytes(pdfPath), "application/pdf", "document.pdf");
        //}

        //private readonly string _contractsPath = @"C:\Users\uinformatica6.GIGHA\OneDrive - GIGHA SAS - JIRO SAS\Documentos\Dev\archive\contratos";

        //    [HttpPost]
        //    public async Task<ActionResult<string>> InsertImageIntoContract(string photoPath,string contractPath)
        //    {
        //        try
        //        {
        //            string newContractPath = Path.Combine(_contractsPath, $"contract_{Guid.NewGuid()}.docx");

        //            Word.Application wordApp = new Word.Application();
        //            wordApp.Visible = false;

        //            //// Open the contract document
        //            //Word.Document doc = wordApp.Documents.Open(contractPath);
        //            //Word.Range range = doc.Range(0, 0);
        //            //Word.InlineShape inlineShape = range.InlineShapes.AddPicture(imagePath);

        //            //// Save the modified document
        //            //doc.SaveAs(newContractPath);
        //            //doc.Close();
        //            //wordApp.Quit();

        //            //// Release COM objects
        //            //Marshal.ReleaseComObject(inlineShape);
        //            //Marshal.ReleaseComObject(range);
        //            //Marshal.ReleaseComObject(doc);
        //            //Marshal.ReleaseComObject(wordApp);

        //            return Ok();
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, ex.Message);
        //        }
        //    }

        //}


        //[HttpPost]
        //public string Post(PaymentClass paymentClass)
        //{
        //    string result = "";
        //    try
        //    {
        //        ApiCobre apiCobre = new ApiCobre(_Configuration);
        //        string Token = apiCobre.PostAuthToken("");
        //        int Balance = apiCobre.GetBalanceBank(Token);

        //        if (Balance > 0)
        //        {
        //            result = apiCobre.PostPayment(Token, paymentClass);
        //        }
                
        //    }
        //    catch (Exception)
        //    {
        //        result = "401";
        //    }
        //    return result;
        //}

        //[HttpPost]
        //public string Post([FromHeader] string Token, [FromBody] PaymentClass paymentClass)
        //{
        //    string result = "";
        //    try
        //    {
        //        ApiCobre apiCobre = new ApiCobre(_Configuration);

        //        result = apiCobre.PostPayment(Token, paymentClass);
        //    }
        //    catch (Exception)
        //    {
        //        result = "401";
        //    }
        //    return result;
        //}

        //[HttpPut]
        //public async Task<string> Put([FromHeader] string Token)
        //{
        //    string result = "";
        //    try
        //    {
        //        ApiCobre apiCobre = new ApiCobre(_Configuration);

        //        //result = apiCobre.PostPaymentAsync(Token);
        //    }
        //    catch (Exception)
        //    {
        //        result = "401";
        //    }
        //    return result;
        //}
    }
}