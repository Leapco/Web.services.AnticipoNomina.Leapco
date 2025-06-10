using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Security.Claims;
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
                string code = "1";

                DataTable dataUser = userModel.PostRecoveryCode(ID, code);
                responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();
                responseModels.CodeResponse = "200";
                if (dataUser.Rows[0]["code"].ToString() == "1")
                {
                    SecurityCore securityCore = new(_configuration);
                    string Link = "https://anticiposdenomina.azurewebsites.net/";
                    string TokenEmail = utilities.GetSHA256(ID + _configuration["JwtSettings:SalKeyChangePass"]);
                    Link = Link + $"?id={ID}&token={TokenEmail}";
                    string bodyEmail = utilities.GetBodyEmailCode(Link, dataUser,1);

                    utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Recuperacion de contraseña", bodyEmail, true, "");
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al procesar la recuperacion. Intente nuevamente.";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels PutPassword(UpdatePasswordRequest UpdatePasswordRequest, string Token)
        {
            ResponseModels responseModels = new();
            Utilities utilities = new(_configuration);
            try
            {
                SecurityCore securityCore1 = new(_configuration);
                if (Token == utilities.GetSHA256(UpdatePasswordRequest.ID + _configuration["JwtSettings:SalKeyChangePass"]))
                {
                    UserModel userModel = new(_configuration);
                    UpdatePasswordRequest.NewPasswordText = UpdatePasswordRequest.NewPassword;
                    UpdatePasswordRequest.NewPassword = utilities.GetSHA256(UpdatePasswordRequest.NewPassword);
                    DataTable dataUser = userModel.PutPassword(UpdatePasswordRequest, 1);

                    responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                    if (dataUser.Rows[0]["code"].ToString() == "1")
                    {
                        responseModels.Token = Token;
                        responseModels.CodeResponse = "201";
                    }
                    else
                        responseModels.CodeResponse = "204";
                }
                else
                {
                    responseModels.MessageResponse = "Token expirado";
                    responseModels.CodeResponse = "401";
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de codigo";
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
                    string bodyMessage = utilities.GetBodyEmailCode(code, dataUser, 1);

                    if (activateUserResponse.Email.Count() > 5)
                        utilities.SendEmail(dataUser.Rows[0]["email"].ToString(), "Activar usuario", bodyMessage, true, "");
                    else
                        utilities.SendSms(activateUserResponse.CellPhone, "Codigo de activacion es: " + code);

                    responseModels.Token = securityCore.GenerateToken(activateUserResponse.ID, "");
                    responseModels.CodeResponse = "201";
                }
                else
                    responseModels.CodeResponse = "200";
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels PutPasswordActivate(UpdatePasswordRequest UpdatePasswordRequest, string Token)
        {
            ResponseModels responseModels = new();
            try
            {
                responseModels.MessageResponse = "Token expirado";
                responseModels.CodeResponse = "401";

                SecurityCore securityCore1 = new(_configuration);
                var (isValid, claimsPrincipal) = securityCore1.IsTokenValid(Token);

                if (isValid)
                {
                    var IDToken = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                    if (UpdatePasswordRequest.ID == IDToken)
                    {
                        UserModel userModel = new(_configuration);
                        Utilities utilities = new(_configuration);
                        UpdatePasswordRequest.NewPasswordText = UpdatePasswordRequest.NewPassword;
                        UpdatePasswordRequest.NewPassword = utilities.GetSHA256(UpdatePasswordRequest.NewPassword);
                        DataTable dataUser = userModel.PutPassword(UpdatePasswordRequest, 2);
                        responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                        switch (dataUser.Rows[0]["code"].ToString())
                        {
                            case "1":
                                responseModels.Token = Token;
                                responseModels.CodeResponse = "201";
                                break;
                            case "2":
                                responseModels.CodeResponse = "200";
                                break;
                            case "3":
                                // codigo incorrecto
                                responseModels.CodeResponse = "202";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseLoginModels GetDataGeneral(string ID, int Option, string Token)
        {
            ResponseLoginModels responseModels = new();
            try
            {
                responseModels.MessageResponse = "Token expirado";
                responseModels.CodeResponse = "401";

                SecurityCore securityCore1 = new(_configuration);
                var (isValid, claimsPrincipal) = securityCore1.IsTokenValid(Token);

                if (isValid)
                {
                    var IDToken = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
                    if ( ID == IDToken)
                    {
                        UserModel userModel = new(_configuration);
                        DataTable dataUser = userModel.GetDataGeneral(ID, 1);
                        responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                        if (dataUser.Rows[0]["code"].ToString() == "1")
                        {
                            UtilitiesCore utilitiesCore = new();

                            responseModels.Token = Token;
                            responseModels.CodeResponse = "200";
                            responseModels.Data = utilitiesCore.GetDataUser(dataUser);
                        }
                        else
                            responseModels.CodeResponse = "200";
                    }
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al consultar.";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels ValidateActiveUser(string ID, string Code, string Token)
        {
            ResponseModels responseModels = new();
            try
            {
                responseModels.MessageResponse = "Token expirado";
                responseModels.CodeResponse = "401";

                SecurityCore securityCore1 = new(_configuration);
                var (isValid, claimsPrincipal) = securityCore1.IsTokenValid(Token);

                if (isValid)
                {
                    UserModel userModel = new(_configuration);
                    UpdatePasswordRequest updatePasswordRequest = new()
                    {
                        ID = ID,
                        Code = Code
                    };
                    DataTable data= userModel.PutPassword(updatePasswordRequest, 3);
                    responseModels.MessageResponse = data.Rows[0]["msg"].ToString();
                    if (data.Rows[0]["code"].ToString() == "1")
                    {
                        responseModels.Token = Token;
                        responseModels.CodeResponse = "200";
                    }
                    else
                        responseModels.CodeResponse = "204";                   
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al consultar.";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
        public ResponseModels ValidateChangePassword(string ID, string Token)
        {
            ResponseModels responseModels = new();
            Utilities utilities = new(_configuration);
            try
            {
                SecurityCore securityCore1 = new(_configuration);
                if (Token == utilities.GetSHA256(ID + _configuration["JwtSettings:SalKeyChangePass"]))
                {
                    UserModel userModel = new(_configuration);
                    UpdatePasswordRequest updatePasswordRequest = new()
                    {
                        ID = ID
                    };
                    DataTable dataUser = userModel.PutPassword(updatePasswordRequest, 4);
                    responseModels.MessageResponse = dataUser.Rows[0]["msg"].ToString();

                    if (dataUser.Rows[0]["code"].ToString() == "1")
                    {
                        responseModels.Token = Token;
                        responseModels.CodeResponse = "200";
                    }
                    else
                        responseModels.CodeResponse = "204";
                }
                else
                {
                    responseModels.MessageResponse = "Token expirado";
                    responseModels.CodeResponse = "401";
                }
            }
            catch (Exception)
            {
                responseModels.MessageResponse = "Error al envio de codigo";
                responseModels.CodeResponse = "500";
            }
            return responseModels;
        }
    }
}