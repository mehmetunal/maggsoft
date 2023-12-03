using System.ComponentModel;

namespace Maggsoft.Framework.Security.Model;

/*
 * appsettings.json
 "TokenOptions": {
    "AccessTokenExpiration": 2,
    "ApiName": "Elearning Api",
    "ApiVersion": "v1",
    "ApiBaseUrl": "https://localhost:5001",
    "IdentityServerBaseUrl": "https://is4user.seyirone.com",
    "OidcSwaggerUIClientId": "elearning_api_swaggerui",
    "OidcApiName": "elearning_api",
    "AdministrationRole": "Administrator",
    "RequireHttpsMetadata": true,
    "CorsAllowAnyOrigin": true,
    "CorsAllowOrigins": [],
    "IgnoreUrls":[]
},
 */
public class ApiTokenOptions
{
    public string ApiName { get; set; }
    public string ApiVersion { get; set; }
    public string IdentityServerBaseUrl { get; set; }
    public string ApiBaseUrl { get; set; }
    public string OidcSwaggerUIClientId { get; set; }
    public bool RequireHttpsMetadata { get; set; }
    public string OidcApiName { get; set; }
    public string AdministrationRole { get; set; }
    public bool CorsAllowAnyOrigin { get; set; }
    public string[] CorsAllowOrigins { get; set; }
    public int AccessTokenExpiration { get; set; }

    [DefaultValue("Qqert___---&/^+%^+/&)((/=)IDFXGAS'34slşfkdsf.asdasdfdgdf.g..hg.jy.uı.yuı.yu..dxc.v.xzc.sarf.erwt..hgf.f.gh.fgh.fgg.1.23.123..435.46.45.!!!!&+%&'^+&//()YGFDBSDF%++^%'^!'^(/))/(=GDZXC'")]
    public string SecurityKey { get; set; }
    public string[] IgnoreUrls { get; set; }
}