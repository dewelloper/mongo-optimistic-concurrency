using System.Security.Claims;

namespace HMTSolution.BCS.Controllers
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public ClaimsIdentity RoleName { get; internal set; }
    }
}