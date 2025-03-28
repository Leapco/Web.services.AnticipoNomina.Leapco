namespace WebServicesAnticiposNomina.Models.Class.Request
{
    public class SiigoRequest
    {
    }
    public class FacturaVentaRequest
    {
        public Documento Document { get; set; }
        public string Date { get; set; }
        public Cliente Customer { get; set; }
        public int CostCenter { get; set; }
        public Moneda Currency { get; set; }
        public int Seller { get; set; }
        public Estado Stamp { get; set; }
        public Estado Mail { get; set; }
        public string Observations { get; set; }
        public List<Item> Items { get; set; }
        public List<Pago> Payments { get; set; }
        public List<DescuentoGlobal> GlobalDiscounts { get; set; }
        public Dictionary<string, object> AdditionalFields { get; set; }
    }
    public class Documento
    {
        public int Id { get; set; }
    }
    public class Cliente
    {
        public string PersonType { get; set; }
        public string IdType { get; set; }
        public string Identification { get; set; }
        public int BranchOffice { get; set; }
        public List<string> Name { get; set; }
        public Direccion Address { get; set; }
        public List<Telefono> Phones { get; set; }
        public List<Contacto> Contacts { get; set; }
    }
    public class Direccion
    {
        public string Address { get; set; }
        public Ciudad City { get; set; }
        public string PostalCode { get; set; }
    }
    public class Ciudad
    {
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
    }

    public class Telefono
    {
        public string Indicative { get; set; }
        public string Number { get; set; }
        public string Extension { get; set; }
    }
    public class Contacto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Telefono Phone { get; set; }
    }
    public class Moneda
    {
        public string Code { get; set; }
        public decimal ExchangeRate { get; set; }
    }
    public class Estado
    {
        public bool Send { get; set; }
    }
    public class Item
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public List<Impuesto> Taxes { get; set; }
        public Transporte Transport { get; set; }
    }
    public class Impuesto
    {
        public int Id { get; set; }
    }
    public class Transporte
    {
        public int FileNumber { get; set; }
        public string ShipmentNumber { get; set; }
        public int TransportedQuantity { get; set; }
        public string MeasurementUnit { get; set; }
        public decimal FreightValue { get; set; }
        public string PurchaseOrder { get; set; }
        public string ServiceType { get; set; }
    }
    public class Pago
    {
        public int Id { get; set; }
        public decimal Value { get; set; }
        public string DueDate { get; set; }
    }
    public class DescuentoGlobal
    {
        public int Id { get; set; }
        public decimal Percentage { get; set; }
        public decimal Value { get; set; }
    }
}
