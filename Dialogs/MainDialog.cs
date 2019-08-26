using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotFrameworkSample.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BotFrameworkSample.Dialogs
{
    public class MainDialog: ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly int _minutosVencerNumero;

        public MainDialog(BotStateService botStateService, int minutosVencerNumero): base(nameof(MainDialog))
        {
            _botStateService = botStateService;
            _minutosVencerNumero = minutosVencerNumero;
            InicializarCascada();
        }

        private void InicializarCascada()
        {
            WaterfallStep[] steps = new WaterfallStep[]
            {
                InitialSteAsync,
                FinalStepAsync
            };

            //Agrego los dialogos a utilizar
            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", steps));
            AddDialog(new ObtenerReporteDialog($"{nameof(MainDialog)}.obtenerReporte", _botStateService, _minutosVencerNumero));
            
            //Seteo el id del dialogo inicial
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";

        }

        private async Task<DialogTurnResult> InitialSteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string from = stepContext.Context.Activity.From.Id;

            string to = stepContext.Context.Activity.Recipient.Id;

            //Damos una bienvenida al usuario y llamamos al dialogo correcto
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Bienvenido al servicio de Reportes"),
                cancellationToken);

            return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.obtenerReporte", null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Le decimos al usaurio que finalizo el proceso y que gracias por particiapar V:
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Gracias por usar nuestro servicio, un gusto atenderle!"), cancellationToken);

            //Mando al terminar el main dialog
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
