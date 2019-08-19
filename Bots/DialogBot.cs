using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotFrameworkSample.Helpers;
using BotFrameworkSample.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace BotFrameworkSample.Bots
{
    /// <summary>
    /// Clase generica para iniciar dialogos de cualquier tipo
    /// </summary>
    public class DialogBot<T>: ActivityHandler where T: Dialog
    {
        private readonly Dialog _dialog;
        private readonly BotStateService _botStateService;
        private readonly ILogger _logger;


        public DialogBot(Dialog dialog, BotStateService botStateService, ILogger<DialogBot<T>> logger)
        {
            _dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
        {
            //Llamamos al base para que haga lo normal
            await base.OnTurnAsync(turnContext, cancellationToken);

            //Mandamos a guardar el estado de cualquier cambio sufrido durante el Turn
            await _botStateService.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _botStateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Corriendo el dialogo con Message Activity");

            //Llamamos nuestro metodo run que correra nuestro dialogo
            await _dialog.Run(turnContext, _botStateService.DialogStateAccessor, cancellationToken);
        }
    }
}
