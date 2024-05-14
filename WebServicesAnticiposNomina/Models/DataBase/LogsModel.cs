using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Data;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;

namespace WebServicesAnticiposNomina.Models.DataBase
{
    public class LogsModel
    {
        private ConnectionModel dbConnection;
        private readonly IConfiguration _configuration;

        public LogsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DataTable PostLog(LogRequest logRequest)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("Id_Anticipo", logRequest.Id_Anticipo, DbType.Int64),
                    dbConnection.CreateParam("Id_cliente", logRequest.Id_cliente, DbType.String),
                        dbConnection.CreateParam("Observacion", logRequest.Observacion, DbType.String),
                        dbConnection.CreateParam("Request_json", logRequest.Request_json, DbType.String),
                        dbConnection.CreateParam("Origen", logRequest.Origen, DbType.Int64)
                };
                return dbConnection.GetDataTable("ProcesoAnticipo", parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
