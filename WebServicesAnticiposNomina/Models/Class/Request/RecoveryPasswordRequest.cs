namespace WebServicesAnticiposNomina.Models.Class.Request
{
    public class UpdatePasswordRequest
    {
        public string? ID { get; set; }
        public string? NewPassword { get; set; }
        public string? Code { get; set; }
    }
}
