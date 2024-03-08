namespace WebServicesAnticiposNomina.Models.Class
{
    public class EmpleadoClass
    {
        public string identificacion { get; set; }
        public string nombre_completo { get; set; }
        public string celular { get; set; }
        public string correo { get; set; }
        public string fecha_ingreso { get; set; }
        public string numero_cuenta { get; set; }
        public string tipo_cuenta { get; set; }
        public string contrato { get; set; }
        public string salario_mes { get; set; }
        public string codigo_banco { get; set; }
        public string banco { get; set; }
        public string tipo_identificacion { get; set; }
        public int dias_laborados { get; set; }
        public string total_anticipado { get; set; }
        public int numero_anticipos { get; set; }
        public int numero_aprobado { get; set; }
        public int numero_rechazado { get; set; }
    }
}
