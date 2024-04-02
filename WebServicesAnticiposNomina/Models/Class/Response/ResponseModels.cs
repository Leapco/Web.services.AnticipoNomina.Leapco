namespace WebServicesAnticiposNomina.Models.Class.Response
{
    public class ResponseModels
    {
        public string? CodeResponse { get; set; } = string.Empty;
        public string? MessageResponse { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? Data { get; set; } = string.Empty;
        public dynamic? DataApiCobre { get; set; }
    }
}
