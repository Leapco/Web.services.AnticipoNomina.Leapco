namespace WebServicesAnticiposNomina.Models.Class
{
    public class CounterpartyResponse
    {
        public int total_items { get; set; }
        public int total_pages { get; set; }
        public bool is_last_page { get; set; }
        public int page_items { get; set; }
        public List<CounterpartyContent> Contents { get; set; }
    }
    public class CounterpartyContent
    {
        public string id { get; set; }
        public string geo { get; set; }
        public string type { get; set; }
        public string alias { get; set; }
        public MetadataCounterparty metadata { get; set; }
    }
    public class MetadataCounterparty
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
