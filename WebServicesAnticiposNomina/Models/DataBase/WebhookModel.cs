using Microsoft.Data.SqlClient;
using System.Data;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;

namespace WebServicesAnticiposNomina.Models.DataBase
{
    public class WebhookModel
    {
        private ConnectionModel dbConnection;
        private readonly IConfiguration _configuration;

        public WebhookModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DataTable GetProcesoWebhook(int idAnticipo, int Option)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("ID", 0, DbType.Int64),
                    dbConnection.CreateParam("Id_Anticipo", idAnticipo, DbType.Int64),
                    dbConnection.CreateParam("Id_cliente", 0, DbType.Int64),
                    dbConnection.CreateParam("Description", "", DbType.String),
                    dbConnection.CreateParam("Url", "", DbType.String),
                    dbConnection.CreateParam("Option", Option, DbType.Int64)
                };
                return dbConnection.GetDataTable("ProcesoWebhook", parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
