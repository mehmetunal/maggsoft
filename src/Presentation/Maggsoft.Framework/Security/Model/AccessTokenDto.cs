using System;

namespace Maggsoft.Framework.Security.Model;

public class AccessTokenDto
{ 
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; } 
    public object User { get; set; }
}
