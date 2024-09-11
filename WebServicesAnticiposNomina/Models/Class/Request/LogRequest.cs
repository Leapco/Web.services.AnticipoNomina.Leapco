namespace WebServicesAnticiposNomina.Models.Class.Request
{
    public class LogRequest
    {
        public int Id_Anticipo { get; set; }
        public int Id_cliente { get; set; }
        public string? Observacion { get; set; }
        public string? Request_json { get; set; }
        public string? Origen { get; set; }
    }
}
