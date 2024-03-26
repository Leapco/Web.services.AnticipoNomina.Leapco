namespace WebServicesAnticiposNomina.Models.Class
{
    public class Beneficiary
    {
        public string documentType { get; set; }
        public string documentNumber { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public BankInfo bankInfo { get; set; }
    }

    public class BankInfo
    {
        public string bankCode { get; set; }
        public string accountType { get; set; }
        public string accountNumber { get; set; }
    }

    public class NoveltyDetail
    {
        public string type { get; set; }
        public decimal totalAmount { get; set; }
        public string description { get; set; }
        public string descriptionExtra1 { get; set; }
        public string descriptionExtra2 { get; set; }
        public string descriptionExtra3 { get; set; }
        public string dueDate { get; set; }
        public string reference { get; set; }
        public Beneficiary beneficiary { get; set; }
    }

    public class PaymentClass
    {
        public int controlRecord { get; set; }
        public List<NoveltyDetail> noveltyDetails { get; set; }
    }

}
