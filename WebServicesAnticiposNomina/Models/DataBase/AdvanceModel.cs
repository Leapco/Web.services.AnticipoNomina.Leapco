using Microsoft.Data.SqlClient;
using System.Data;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;

namespace WebServicesAnticiposNomina.Models.DataBase
{
    public class AdvanceModel
    {
        private ConnectionModel dbConnection;
        private readonly IConfiguration _configuration;

        public AdvanceModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DataTable PostAdvance(AdvanceRequest AdvanceRequest, int Option)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("ID", AdvanceRequest.ID, DbType.String),
                    dbConnection.CreateParam("Code", AdvanceRequest.Code, DbType.String),
                    dbConnection.CreateParam("AdvanceAmount", AdvanceRequest.AdvanceAmount, DbType.String),
                    dbConnection.CreateParam("uuid", AdvanceRequest.uuid, DbType.String),
                    dbConnection.CreateParam("Option", Option, DbType.Int64)
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
