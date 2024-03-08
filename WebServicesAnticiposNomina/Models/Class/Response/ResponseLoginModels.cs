namespace WebServicesAnticiposNomina.Models.Class.Response
{   
    public class ResponseLoginModels
    {
        public string CodeResponse { get; set; } = string.Empty;
        public string MessageResponse { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DataUser Data { get; set; }
    }
}
