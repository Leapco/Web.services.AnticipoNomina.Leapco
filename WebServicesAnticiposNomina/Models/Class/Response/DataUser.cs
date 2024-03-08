namespace WebServicesAnticiposNomina.Models.Class.Response
{
    public class DataUser
    {
        public decimal? SaldoDisponible { get; set; }
        public EmpleadoClass? DataEmpleado { get; set; }
        public ParametrosClienteClass? DataParametrosCliente { get; set; }
        public List<AnticipoClass>? DataAnticipos { get; set; }
    }
}
