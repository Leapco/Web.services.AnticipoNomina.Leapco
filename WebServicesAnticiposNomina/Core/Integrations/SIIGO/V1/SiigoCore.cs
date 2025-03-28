using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;
using WebServicesAnticiposNomina.Core.Integrations.PaymentGateway.V2;
using WebServicesAnticiposNomina.Models.Class;
using WebServicesAnticiposNomina.Models.DataBase;

namespace WebServicesAnticiposNomina.Core.Integrations.SIIGO.V1
{
    public class SiigoCore
    {
        private readonly IConfiguration _configuration;

        public SiigoCore(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string PostAuthSiigo(CredentialsClass credentialsClass)
        {
            using (var _httpClient = new HttpClient())
            {
                credentialsClass.username = "siigoapi@pruebas.com";
                credentialsClass.access_key = "OWE1OGNkY2QtZGY4ZC00Nzg1LThlZGYtNmExMzUzMmE4Yzc1Omt2YS4yJTUyQEU=";

                var jsonContent = JsonConvert.SerializeObject(credentialsClass);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                string rout = "https://api.siigo.com/auth";
                string accessToken = "NULL";

                try
                {
                    using var response = _httpClient.PostAsync(rout, content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        accessToken = jsonObject.access_token;
                    }
                    return accessToken;
                }
                catch (Exception ex)
                {
                    return accessToken;
                }
            }
        }
        public string PostCounterPartyClientesCobre(int id_cliente)
        {
            // Consulta datos cliente
            SiigoModel siigoModel = new(_configuration);
            DataTable dataUser = siigoModel.GetProcesoSiigo(id_cliente, "", 1);

            // Registrarlo en cobre
            // autenticacion
            ApiCobre_v3 apiCobre = new(_configuration);
            string TokenApi = apiCobre.PostAuthToken(dataUser);
            if (TokenApi != "false")
            {
                // crear counterparty
                string destination_id = apiCobre.PostCounterParty(TokenApi, dataUser);
                if (destination_id != null)
                {
                    _ = siigoModel.GetProcesoSiigo(id_cliente, destination_id, 2);
                }
                return "Cliente creado en cobre";
            }
            else
            {
                return "Error al crear cliente en cobre";
            }
        }
    }
}
