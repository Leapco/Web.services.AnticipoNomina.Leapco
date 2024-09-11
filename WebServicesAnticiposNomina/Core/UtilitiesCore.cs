using Newtonsoft.Json;
using WebServicesAnticiposNomina.Models.Class.Response;
using WebServicesAnticiposNomina.Models.Class;
using System.Data;

namespace WebServicesAnticiposNomina.Core
{
    public class UtilitiesCore
    {
        public DataUser GetDataUser(DataTable dataLogin)
        {

            EmpleadoClass? empleado = JsonConvert.DeserializeObject<EmpleadoClass>(dataLogin.Rows[0]["empleado"].ToString());
            ParametrosClienteClass? parametrosCliente = JsonConvert.DeserializeObject<ParametrosClienteClass>(dataLogin.Rows[0]["parametros_cliente"].ToString());
            List<AnticipoClass>? Anticipos = JsonConvert.DeserializeObject<List<AnticipoClass>>(dataLogin.Rows[0]["anticipos"].ToString());

            DataUser dataUser = new()
            {
                SaldoDisponible = Convert.ToDecimal(dataLogin.Rows[0]["saldoDisponible"]),
                DataEmpleado = empleado,
                DataParametrosCliente = parametrosCliente,
                DataAnticipos = Anticipos
            };
            return dataUser;
        }
    }
}
