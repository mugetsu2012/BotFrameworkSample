using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotFrameworkSample.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace BotFrameworkSample.Dialogs
{
    public class PreguntarReintentoDialog: ComponentDialog
    {
        private readonly BotStateService _botStateService;

        public PreguntarReintentoDialog(string dialogId, BotStateService botStateService): base(dialogId)
        {
            _botStateService = botStateService;

            InicializarCascada();
        }

        private void InicializarCascada()
        {
            WaterfallStep[] steps = new WaterfallStep[]
            {
                PreguntarReintentoStepAsync,
                ProcesarRespuestaReintentoStepAsync,
                FinalizarStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(PreguntarReintentoDialog)}.mainFlow", steps));
            AddDialog(new ChoicePrompt($"{nameof(PreguntarReintentoDialog)}.pregunta"));
            AddDialog(new ReIntentarDialog($"{nameof(PreguntarReintentoDialog)}.reintentar", _botStateService));
            AddDialog(new NuevoCodigoDialog($"{nameof(PreguntarReintentoDialog)}.nuevoCodigo", _botStateService));

            InitialDialogId = $"{nameof(PreguntarReintentoDialog)}.mainFlow";

        }

        /// <summary>
        /// En este paso debo preguntarle al usuario si desea re intentar o que quiere un nuevo codigo
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> PreguntarReintentoStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(PreguntarReintentoDialog)}.pregunta", new PromptOptions()
            {
                Prompt = MessageFactory.Text("El codigo ingresado no es valido. ¿Desea reintentar o que se le envie un nuevo codigo? "),
                Choices = ChoiceFactory.ToChoices(new List<string>() { "Reintentar", "Nuevo"})
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcesarRespuestaReintentoStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Extramos el dato de la opcion anterior
            string eleccion = ((FoundChoice)stepContext.Result).Value;

            //Verificamos que fue lo que eligio
            if (eleccion == "Reintentar")
            {
                //Si pidio reintentar, levanto el dialogo de re intento
                return await stepContext.BeginDialogAsync($"{nameof(PreguntarReintentoDialog)}.reintentar", null,
                    cancellationToken);
            }
            else
            {
                //Si dijo que quiere un codigo nuevo, se le manda al dialogo de nuevo codigo
                return await stepContext.BeginDialogAsync($"{nameof(PreguntarReintentoDialog)}.nuevoCodigo", null,
                    cancellationToken);

            }
        }

        /// <summary>
        /// Entrara aca cuando finalicemos el proceso de ingreso de OTP 
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> FinalizarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool exito = (bool) stepContext.Result;

            if (exito)
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync($"{nameof(PreguntarReintentoDialog)}.mainFlow", null, cancellationToken);
            }


            
        }

    }
}
