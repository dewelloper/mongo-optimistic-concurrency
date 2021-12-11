using HMTSolution.BCS.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HMTSolution.BCS.InternalServices
{
    public static class TokenGenerator
    {
        public static async Task<string> GenerateApiUserToken(UserInfo user, IConfiguration _configuration)
        {
            string signingKey = _configuration.GetValue<string>("Jwt:Key");
            string issuer = _configuration.GetValue<string>("Jwt:Issuer");
            int hours = _configuration.GetValue<int>("Jwt:HoursValid");
            System.DateTime expireDateTime = System.DateTime.UtcNow.AddHours(hours);

            byte[] signingKeyBytes = System.Text.Encoding.UTF8.GetBytes(signingKey);
            SymmetricSecurityKey secKey = new SymmetricSecurityKey(signingKeyBytes);
            SigningCredentials creds = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256);

            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName)//,
        //new Claim(ClaimTypes.Role, user.RoleName)
    };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: issuer,
                claims: authClaims,
                expires: System.DateTime.UtcNow.AddHours(hours),
                signingCredentials: creds
            );
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            string writtenToken = handler.WriteToken(token);

            return await Task.FromResult(writtenToken);
        }
    }
}
