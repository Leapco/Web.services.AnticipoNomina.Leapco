using Newtonsoft.Json;
using System.Text;
using WebServicesAnticiposNomina.Models.Class;

namespace WebServicesAnticiposNomina.Core.Integrations.SIIGO.V1
{
    public class SiigoCore
    {
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
    }
}
