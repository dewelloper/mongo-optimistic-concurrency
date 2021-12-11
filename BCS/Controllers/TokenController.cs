using HMTSolution.BCS.InternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HMTSolution.BCS.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous()]
    public class TokenController : Controller
    {
        IConfiguration _configuration;
        public TokenController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //Token Generate etme işi statik bir sınıfa verildi ve identity destekli hale getirildi JWT konfigürasyonlar 
        //appsettings kısmından alınmaya başlandı
        [HttpPost("new")]
        public async Task<IActionResult> GetTokenAsync([FromBody] UserInfo user)
        {
            try
            {
                return new ObjectResult(await TokenGenerator.GenerateApiUserToken(user, _configuration));
            }
            catch (Exception ex)
            { }
            
            return Unauthorized();
        }

    }

}
