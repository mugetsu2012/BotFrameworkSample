using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;

namespace BotFrameworkSample
{
    /// <summary>
    /// Esta clase implementa el provider de credenciales para cuando se esta en produccion
    /// </summary>
    public class ConfigurationCredentialProvider: SimpleCredentialProvider
    {
        public ConfigurationCredentialProvider(IConfiguration configuration): 
            base(configuration["MicrosoftAppId"], configuration["MicrosoftAppPassword"])
        {
            
        }
    }
}
