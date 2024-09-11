using Microsoft.Data.SqlClient;
using System.Data;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;

namespace WebServicesAnticiposNomina.Models.SQLServer
{
    public class UserModel
    {
        private ConnectionModel dbConnection;
        private readonly IConfiguration _configuration;

        public UserModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DataTable Login(LoginRequest loginRequest)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("UserName", loginRequest.UserName, DbType.String),
                    dbConnection.CreateParam("Password", loginRequest.Password, DbType.String)
                };
                return dbConnection.GetDataTable("LoginApp", parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable PostRecoveryCode(string UserId, string Code)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("UserId", UserId, DbType.String),
                    dbConnection.CreateParam("Code", Code, DbType.String)
                };
                return dbConnection.GetDataTable("ValidaUsuarioCodigoRecuperacion", parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable PutPassword(UpdatePasswordRequest UpdatePasswordRequest, int Option)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("UserId", UpdatePasswordRequest.ID, DbType.String),
                    dbConnection.CreateParam("NewPassword", UpdatePasswordRequest.NewPassword, DbType.String),
                    dbConnection.CreateParam("Option", Option, DbType.String)
                };
                return dbConnection.GetDataTable("ActualizarClaveUsuario", parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable PostActiveteCode(ActivateUserResponse activateUserResponse, string Code)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("UserId", activateUserResponse.ID, DbType.String),
                    dbConnection.CreateParam("CellPhone", activateUserResponse.CellPhone, DbType.String),
                    dbConnection.CreateParam("Code", Code, DbType.String)
                };
                return dbConnection.GetDataTable("ValidaEmpleadoCodigo", parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetDataGeneral(string ID, int Option)
        {
            try
            {
                dbConnection = new ConnectionModel(_configuration);

                List<SqlParameter> parameters = new()
                {
                    dbConnection.CreateParam("ID", ID, DbType.String),
                    dbConnection.CreateParam("Option", Option, DbType.Int16)
                };
                return dbConnection.GetDataTable("ConsultarDatosGenerales", parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
