using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebServicesAnticiposNomina.Core
{
    public class SecurityCore
    {
        private readonly IConfiguration _configuration;
        public SecurityCore(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(string username, string data)
        {
            try
            {
                var key = new byte[32];
                key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new(ClaimTypes.Name, username),
                        new(ClaimTypes.UserData, data)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                return ($"Error: {ex.Message}");
            }
        }
        public bool IsTokenValid(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return false;

                var key = new byte[32];
                key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                // Verificar la fecha de expiración
                if (securityToken.ValidTo < DateTime.UtcNow) return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
