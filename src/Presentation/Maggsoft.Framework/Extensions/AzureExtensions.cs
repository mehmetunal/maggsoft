using Maggsoft.Framework.Model;
using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
namespace Maggsoft.Framework.Extensions
{
    public static class AzureExtensions
    {
        /// <summary>
        ///    "AzureKeyVaultConfiguration": {
        //          "AzureKeyVaultEndpoint": "",
        //          "ClientId": "",
        //          "ClientSecret": "",
        //          "TenantId": "",
        //          "UseClientCredentials": true,
        //          "DataProtectionKeyIdentifier": "",
        //          "ReadConfigurationFromKeyVault": false
        //      }
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="configurationBuilder"></param>
        public static void AddAzureKeyVaultConfiguration(this IConfiguration configuration, IConfigurationBuilder configurationBuilder)
        {
            if (configuration.GetSection(nameof(AzureKeyVaultConfiguration)).Exists())
            {
                var azureKeyVaultConfiguration = configuration.GetSection(nameof(AzureKeyVaultConfiguration)).Get<AzureKeyVaultConfiguration>();

                if (azureKeyVaultConfiguration.ReadConfigurationFromKeyVault)
                {
                    if (azureKeyVaultConfiguration.UseClientCredentials)
                    {
                        configurationBuilder.AddAzureKeyVault(azureKeyVaultConfiguration.AzureKeyVaultEndpoint,
                            azureKeyVaultConfiguration.ClientId, azureKeyVaultConfiguration.ClientSecret);
                    }
                    else
                    {
                        var keyVaultClient = new KeyVaultClient(
                            new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider()
                                .KeyVaultTokenCallback));

                        configurationBuilder.AddAzureKeyVault(azureKeyVaultConfiguration.AzureKeyVaultEndpoint,
                            keyVaultClient, new DefaultKeyVaultSecretManager());
                    }
                }
            }
        }
    }
}