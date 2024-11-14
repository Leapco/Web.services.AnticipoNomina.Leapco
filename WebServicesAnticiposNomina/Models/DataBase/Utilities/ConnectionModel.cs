using Microsoft.Data.SqlClient;
using System.Data;

namespace WebServicesAnticiposNomina.Models.DataBase.Utilities
{
    public class ConnectionModel
    {
        private readonly IConfiguration _configuration;

        private readonly string Connection;
        public ConnectionModel(IConfiguration configuration)
        {
            Utilities utilities = new Utilities(configuration);
            Connection = utilities.RetornarSetting();
            _configuration = configuration;
        }
        public DataTable GetDataTable(string NameSP, List<SqlParameter> ListParam)
        {
            return GetDataSet(NameSP, ListParam).Copy().Tables[0];
        }
        public SqlParameter CreateParam(string parameterName, object value, DbType type)
        {
            SqlParameter parametro = new()
            {
                ParameterName = "@" + parameterName,
                Value = value ?? DBNull.Value,
                DbType = type
            };
            return parametro;
        }
        public DataSet GetDataSet(string NameSP, List<SqlParameter> ListParam)
        {
            using (SqlConnection con = new(Connection))
            {
                DataSet DataSet = new();
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new(NameSP, con))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.StoredProcedure;

                        foreach (SqlParameter param in ListParam)
                        {
                            cmd.Parameters.Add(param);
                        }
                        using (SqlDataAdapter da = new(cmd))
                        {
                            da.Fill(DataSet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities utilities = new(_configuration);
                    utilities.SendEmail("informatica3@gigha.com.co", "error codigo [GetDataSet]", "Error al obtener DataSet. - " + ex.Message, true, "");
                    throw new ApplicationException("Error al obtener DataSet.", ex);
                }
                return DataSet;
            }
        }
    }
}