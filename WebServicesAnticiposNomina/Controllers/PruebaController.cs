using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SixLabors.ImageSharp;
using System.Data;
using System.Net.Http;
using System.Text;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.Class.Request;
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
        [HttpPost]
        public string ADVANCE()
        {
            ApiCobre_v3 apiCobre_V3 = new ApiCobre_v3(_Configuration);

            string TOKEN_ACCES = apiCobre_V3.PostAuthToken_DEV();
            int balance = apiCobre_V3.GetBalanceBank_DEV(TOKEN_ACCES);

            return $"valor de la cuenta de jiro : {balance}";
        }
        ////GET api/<PruebaController>/5
        //[HttpGet]
        //public async Task<string> Get(string celular, string mesaje)
        //{
        //    Utilities utilities = new(_Configuration);
        //    await utilities.SendSms(celular, mesaje);

        //     //utilities.SendEmail("joshuatejada@hotmail.com", "Anticipo generado", "prueba", false, "");

        
        //    return "Mensaje enviado";
        //}
        // GET api/<PruebaController>/5
        //[HttpGet]
        //public async Task<string> GetSms()
        //{
        //    Utilities utilities = new(_Configuration);
        //    utilities.SendSms("3007185717","hola soy yo");
        //    return "Email enviado";
        //}

        //[HttpPost]
        //public async Task<IActionResult> SendSMS()
        //{
        //     HttpClient _httpClient = new HttpClient();
        //    _httpClient.BaseAddress = new Uri("https://dashboard.360nrs.com/api/rest/sms");

        //    var requestBody = "{ \"to\": [\"3007185717\"], \"from\": \"TEST\", \"message\": \"SMS text message\" }";
        //    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        //    _httpClient.DefaultRequestHeaders.Add("Authorization", "Basic  " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{"JIROOTP"}:{"Gsms2024$$"}")));

        //    var response = await _httpClient.PostAsync("", content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        return Ok(responseContent);
        //    }
        //    else
        //    {
        //        return BadRequest("Failed to send SMS");
        //    }
        //}
        //public IActionResult CreateDocument(string imagePath)
        //{
        //    var docPath = @"C:\\Users\\uinformatica6.GIGHA\\OneDrive - GIGHA SAS - JIRO SAS\\Documentos\\Dev\\archive\\contratostemplateContract.docx";
        //    var pdfPath = @"C:\\Users\\uinformatica6.GIGHA\\OneDrive - GIGHA SAS - JIRO SAS\\Documentos\\Dev\\archive\\contratostemplateContract.pdf";

        //    using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(docPath, WordprocessingDocumentType.Docume"nt))
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
        //[HttpPut]
        //public async Task<string> Put([FromHeader] string codigo, int option)
        //{
        //    string result = "";
        //    try
        //    {
        //        Utilities utilities = new Utilities(_Configuration);

        //        result = utilities.EncryptCode(codigo, option);
        //    }
        //    catch (Exception)
        //    {
        //        result = "401";
        //    }
        //    return result;
        //}

        //[HttpPost]
        //public string Post([FromBody] AdvanceRequest advanceRequest)
        //{
        //    string result = "";
        //    try
        //    {
        //        Utilities utilities = new(_Configuration);

        //        utilities.SavePhoto(advanceRequest.Base64Image, 666);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //    return "imagen subida";
        //}
    }
}