using Maggsoft.Core.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;

namespace Maggsoft.Mssql.Services
{
    public class BaseService
    {
        protected virtual Guid? CurrentUserId { get; set; }

        protected virtual string CurrentUserRole => GetClaim(ClaimTypes.Role);

        protected virtual string GetClaim(string key)
        {
            try
            {
                var identity = MaggsoftContext.Current.Resolve<IHttpContextAccessor>()?.HttpContext?.User?.Identity as ClaimsIdentity;
                if (identity == null)
                    return null;

                return identity.FindFirst(p => p.Type == key || p.Issuer == key)?.Value;
            }
            catch
            {
                return null;
            }
        }

        protected virtual string RemoteIp =>
            MaggsoftContext.Current.Resolve<IHttpContextAccessor>().
            HttpContext.Connection.
            RemoteIpAddress.
            ToString();

    }
}
