using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.TransientFaultHandling;

namespace BotFrameworkSample
{
    public class AdapterWithErrorHandler: BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, ILogger<BotFrameworkHttpAdapter> logger): base(configuration, logger)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                //Se loguea el error segun corresponda
                logger.LogError($"Error capturado: {exception.Message}");

                //Se escribe un mensaje de error al usuario
                await turnContext.SendActivityAsync("Lo siento, al parecer ocurrio un error");
            };
        }
    }
}
