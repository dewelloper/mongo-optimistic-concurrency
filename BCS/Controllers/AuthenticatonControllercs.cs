using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HMTSolution.BCS.Controllers
{
    [Microsoft.AspNetCore.Components.Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous()]
    public class TokenController : Controller
    {

        public TokenController()
        {
        }

        [HttpPost("new")]
        public async Task<IActionResult> GetTokenAsync([FromBody] UserInfo user)
        {
            if (await IsValidUserAndPasswordAsync(user.UserName, user.Password))
            {
                // GenerateTokenAsync(user.UserName);
                //var ticketAdForm = GetTicket();
                return new ObjectResult(await GenerateTokenAsync(user.UserName));
            }

            return Unauthorized();
        }

        private static async Task<string> GenerateTokenAsync(string userName)
        {
            var someClaims = new Claim[]{
                new Claim(JwtRegisteredClaimNames.UniqueName,userName),
                new Claim(JwtRegisteredClaimNames.Email, "dewelloper@gmail.com"),
                new Claim(JwtRegisteredClaimNames.NameId, Guid.NewGuid().ToString())
            };

            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("1e16040404a3aef73de4e9eb953f89ad7a74ba21c00dcf0be8148805d931d834"));
            var token = new JwtSecurityToken(
                issuer: "west-world.Stock.com",
                audience: "audience.Stock.com",
                claims: someClaims,
                expires: DateTime.Now.AddMinutes(43200),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            var result = new JwtSecurityTokenHandler().WriteToken(token);
            return await Task.FromResult(result);
        }

        private async Task<bool> IsValidUserAndPasswordAsync(string userName, string password)
        {
            //var isExistUser = await _repoWrapper.User.FindByConditionAsync(k => k.IsApplication && k.UserName == userName && k.Password == password);
            bool isValid = true;
            if (isValid != null)
            {
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        //private static string GetTicket()
        //{
        //    var wr = WebRequest.Create(_gatewayAddress);

        //    wr.Method = "POST";
        //    wr.ContentType = "application/json";

        //    var loginRequest = new
        //    {
        //        grant_type = _grant_type,
        //        client_id = _client_id,
        //        client_secret = _client_secret,
        //        scope = _scope
        //    };

        //    using (var stream = wr.GetRequestStream())
        //    {
        //        WriteObject(stream, loginRequest);
        //    }

        //    var response = (HttpWebResponse)wr.GetResponse();
        //    if (response.StatusCode != HttpStatusCode.OK)
        //    {
        //        throw new InvalidOperationException("Unexpected response status: " + response.StatusCode);
        //    }

        //    using (var stream = response.GetResponseStream())
        //    {
        //        return ReadObject<LoginResponseDto>(stream, (int)response.ContentLength).Ticket;
        //    }
        //}

        //private static void WriteObject(Stream stream, object obj)
        //{
        //    var json = new JavaScriptSerializer().Serialize(obj);
        //    var bytes = Encoding.UTF8.GetBytes(json);
        //    stream.Write(bytes, 0, bytes.Length);
        //}

        //private static T ReadObject<T>(Stream stream, int length)
        //{
        //    var bytes = new byte[length];

        //    var read = 0;
        //    while (read < length)
        //    {
        //        read += stream.Read(bytes, read, length - read);
        //    }

        //    var json = Encoding.UTF8.GetString(bytes);

        //    var serializer = new JavaScriptSerializer();
        //    return serializer.Deserialize<T>(json);
        //}

        class LoginResponseDto
        {
            public string Ticket;
        }
    }

}
