namespace WebServicesAnticiposNomina.Models.Class.Request
{
    public class CounterpartyRequest
    {
        public string geo { get; set; }
        public string type { get; set; }
        public string alias { get; set; }
        public Metadata metadata { get; set; }
    }
    public class Metadata
    {
        public string account_number { get; set; }
        public string counterparty_email { get; set; }
        public string counterparty_phone { get; set; }
        public string counterparty_id_type { get; set; }
        public string counterparty_fullname { get; set; }
        public string counterparty_id_number { get; set; }
        public string beneficiary_institution { get; set; }
    }
}
