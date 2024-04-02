namespace WebServicesAnticiposNomina.Models.Class.Request
{
    public class TransactionRequest
    {
        public string TransactionType { get; set; }
        public string NoveltyUuid { get; set; }
        public string NoveltyDetailUuid { get; set; }
        public DateTime RegisterDate { get; set; }
        public string Status { get; set; }
        public string DescriptionStatus { get; set; }
        public string ClientDocumentType { get; set; }
        public string ClientDocumentNumber { get; set; }
        public string ClientCompleteName { get; set; }
        public string BankCode { get; set; }
        public string BankAccountType { get; set; }
        public string BankAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public string Reference { get; set; }
        public string Checksum { get; set; }
    }
}
