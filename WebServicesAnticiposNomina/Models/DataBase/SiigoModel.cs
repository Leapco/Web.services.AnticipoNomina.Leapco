using Microsoft.Data.SqlClient;
using System.Data;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;

namespace WebServicesAnticiposNomina.Models.DataBase
{
    public class SiigoModel
    {
        private ConnectionModel dbConnection;
        private readonly IConfiguration _configuration;

        public SiigoModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DataTable GetProcesoSiigo(int id_cliente, string destination_id, int Option)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("id_cliente", id_cliente, DbType.Int64),
                    dbConnection.CreateParam("destination_id", destination_id, DbType.String),
                    dbConnection.CreateParam("Option", Option, DbType.Int64)
                };
                return dbConnection.GetDataTable("ProcesoSiigo", parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
