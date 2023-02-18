using Maggsoft.Core.IoC;
using Maggsoft.Framework.Security.Model;

namespace Maggsoft.Framework.Security.Token
{
    public interface IAccessTokenHandler : IService
    {
        /// <summary>
        /// Token Almak İçin
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        AccessTokenDto CreateAccessToken(object user);

        object GetAccessToken();
    }
}
