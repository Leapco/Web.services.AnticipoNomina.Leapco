using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;
using WebServicesAnticiposNomina.Models.SQLServer;

namespace WebServicesAnticiposNomina.Core
{
    public class LoginCore
    {
        public IConfiguration _configuration;

        public LoginCore(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public ResponseLoginModels Login(LoginRequest loginRequest)
        {
            try
            {
                ResponseLoginModels responseModels = new();
                Utilities utilities = new(_configuration);
                UserModel userModel = new(_configuration);
                loginRequest.Password = utilities.GetSHA256(loginRequest.Password);
                DataTable dataLogin = userModel.Login(loginRequest);
                responseModels.MessageResponse = dataLogin.Rows[0].ToString();

                if (dataLogin.Rows.Count > 0)
                {
                    responseModels.MessageResponse = dataLogin.Rows[0]["msg"].ToString();

                    if (dataLogin.Rows[0]["code"].ToString() == "1" || dataLogin.Rows[0]["code"].ToString() == "2")
                    {
                        if (dataLogin.Rows[0]["code"].ToString() == "2")
                        {
                            IntegrationsCore integrationsCore = new();
                            if (!integrationsCore.GetDataFromClient(dataLogin.Rows[0]["api_key"].ToString(), loginRequest.UserName, dataLogin.Rows[0]["ruta_api_empleado"].ToString()))
                            {
                                responseModels.MessageResponse = "Usuario no permitido para el uso de la plataforma";
                                responseModels.CodeResponse = "401";
                            }
                        }

                        SecurityCore securityCore = new(_configuration);
                        string token = securityCore.GenerateToken(loginRequest.UserName, "");

                        responseModels.CodeResponse = "200";
                        responseModels.Token = token;
                        responseModels.Data = this.GetDataUser(dataLogin);
                    }
                    else
                        responseModels.CodeResponse = "401";
                }
                return responseModels;
            }
            catch (Exception)
            {
                throw;
            }
        }
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