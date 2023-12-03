using Maggsoft.Core.IoC;
using Maggsoft.Framework.Security.Model;
using System.Collections.Generic;
using System.Security.Claims;

namespace Maggsoft.Framework.Security.Token;

public interface IAccessTokenHandler : IService
{
    /// <summary>
    /// Token Almak İçin
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    AccessTokenDto CreateAccessToken(object user);

    object GetAccessToken();

    IEnumerable<Claim> GetUserClaims();
}
