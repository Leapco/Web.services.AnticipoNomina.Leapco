using System.Data;
using System.Net.Mail;
using System.Net;
using WebServicesAnticiposNomina.Models.Class.Request;
using WebServicesAnticiposNomina.Models.Class.Response;
using WebServicesAnticiposNomina.Models.DataBase.Utilities;
using WebServicesAnticiposNomina.Models.SQLServer;

namespace WebServicesAnticiposNomina.Core
{
    public class UserCore
    {
        private readonly IConfiguration _configuration;

        public UserCore(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public ResponseModels SendCodeRecovery(string ID)
        {
            ResponseModels responseModels = new();
            try
            {
                UserModel userModel = new(_configuration);
                Utilities utilities = new(_configuration);
                string code = utilities.GenerarCodigo();
                DataTable dataUser = userModel.PostRecoveryCode(ID, code);
                responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                if (dataUser.Rows[0]["code"].ToString() == "1")
                {
                    SecurityCore securityCore = new(_configuration);
                    string bodyEmail = utilities.GetBodyEmailCode(code, dataUser);
                    //string bodyEmail = "Código de recuperación: " + code;
                    utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Recuperacion de contraseña", bodyEmail, true, "");

                    responseModels.Token = securityCore.GenerateToken(ID, "");
                    responseModels.CodeResponse = "201";
                    responseModels.Data = "{'codigo': '" + code + "'}";
                }
                else
                    responseModels.CodeResponse = "200";
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels PutPassword(UpdatePasswordRequest UpdatePasswordRequest, string Token)
        {
            ResponseModels responseModels = new();
            try
            {
                SecurityCore securityCore1 = new(_configuration);
                if (securityCore1.IsTokenValid(Token))
                {
                    UserModel userModel = new(_configuration);
                    Utilities utilities = new(_configuration);
                    UpdatePasswordRequest.NewPassword = utilities.GetSHA256(UpdatePasswordRequest.NewPassword);
                    DataTable dataUser = userModel.PutPassword(UpdatePasswordRequest, 1);
                    responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                    if (dataUser.Rows[0]["code"].ToString() == "1")
                    {
                        responseModels.Token = Token;
                        responseModels.CodeResponse = "201";
                    }
                    else
                        responseModels.CodeResponse = "200";
                }
                else
                {
                    responseModels.MessageResponse = "Token expirado";
                    responseModels.CodeResponse = "401";
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels SendCodeActivate(ActivateUserResponse activateUserResponse)
        {
            ResponseModels responseModels = new();
            try
            {
                UserModel userModel = new(_configuration);
                Utilities utilities = new(_configuration);
                string code = utilities.GenerarCodigo();
                DataTable dataUser = userModel.PostActiveteCode(activateUserResponse, code);
                responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                if (dataUser.Rows[0]["code"].ToString() == "1")
                {
                    SecurityCore securityCore = new(_configuration);
                    string bodyMessage = utilities.GetBodyEmailCode(code, dataUser);

                    if (activateUserResponse.Email.Count() > 5)
                        utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Activar usuario", bodyMessage, true, "");
                    else
                        utilities.SendSms(activateUserResponse.CellPhone, bodyMessage);

                    responseModels.Token = securityCore.GenerateToken(activateUserResponse.ID, "");
                    responseModels.CodeResponse = "201";
                    responseModels.Data = "{'codigo': '" + code + "', 'Email': '" + dataUser.Rows[0]["email"].ToString() + "'}";
                }
                else
                    responseModels.CodeResponse = "200";
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels PutPasswordActivate(UpdatePasswordRequest UpdatePasswordRequest, string Token)
        {
            ResponseModels responseModels = new();
            try
            {
                SecurityCore securityCore1 = new(_configuration);
                if (securityCore1.IsTokenValid(Token))
                {
                    UserModel userModel = new(_configuration);
                    Utilities utilities = new(_configuration);
                    UpdatePasswordRequest.NewPassword = utilities.GetSHA256(UpdatePasswordRequest.NewPassword);
                    DataTable dataUser = userModel.PutPassword(UpdatePasswordRequest, 2);
                    responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                    if (dataUser.Rows[0]["code"].ToString() == "1")
                    {
                        responseModels.Token = Token;
                        responseModels.CodeResponse = "201";
                    }
                    else
                        responseModels.CodeResponse = "200";
                }
                else
                {
                    responseModels.MessageResponse = "Token expirado";
                    responseModels.CodeResponse = "401";
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}