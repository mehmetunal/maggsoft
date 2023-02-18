using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Maggsoft.Framework.Security.Token
{
    public static class SingHandler
    {
        public static SecurityKey GetSecurityKey(string securityKey) =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
    }
}
