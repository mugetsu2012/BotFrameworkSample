using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotFrameworkSample.Model;
using BotFrameworkSample.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BotFrameworkSample.Dialogs
{
    public class ReIntentarDialog: ComponentDialog
    {
        private readonly BotStateService _botStateService;

        public ReIntentarDialog(string dialogId, BotStateService botStateService):base(dialogId)
        {
            _botStateService = botStateService;

            InicializarCascada();
        }

        private void InicializarCascada()
        {
            WaterfallStep[] steps = new WaterfallStep[]
            {
                PedirCodigoStepAsync,
                ValidarOtpStepAsync,
                FinalizarStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(ReIntentarDialog)}.mainFlow", steps));
            AddDialog(new TextPrompt($"{nameof(ReIntentarDialog)}.otp"));
            

            InitialDialogId = $"{nameof(ReIntentarDialog)}.mainFlow";

        }

        private async Task<DialogTurnResult> PedirCodigoStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Le solicitamos al usuario que nos diga el OTP
            //Mando a sacar el objeto de la coversacion donde ya guarde el OTP
            DataConversation dataConversation =
                await _botStateService.DataConversationAccessor.GetAsync(stepContext.Context,
                    () => new DataConversation(), cancellationToken);

            return await stepContext.PromptAsync($"{nameof(ReIntentarDialog)}.otp", new PromptOptions()
            {
                Prompt = MessageFactory.Text(
                    $"Muy bien, ingrese el codigo enviado a su telefono (pssst, el codigo es: {dataConversation.OTP})")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ValidarOtpStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            string otpIngresado = (string)stepContext.Result;
            //Sacamos el OTP que dio el usuario del paso anterior
            stepContext.Values["otp"] = otpIngresado;

            bool esOtpValido =
                await EsOtpValidoAsync(otpIngresado, stepContext.Context, cancellationToken);

            //Si el OTP es valido, lo mando al resumen

            stepContext.Values["exito"] = esOtpValido;
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalizarStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            bool exito = (bool) stepContext.Values["exito"];

            if (exito)
            {
                //Le seteo el OTP al fulano

                // Sacamos al usuario
                PerfilDeUsuario perfilDeUsuario = await _botStateService.PerfilDeUsuarioAccessor.GetAsync(stepContext.Context,
                    () => new PerfilDeUsuario(), cancellationToken);

                perfilDeUsuario.NumeroOTP = (string)stepContext.Values["otp"];

                await _botStateService.PerfilDeUsuarioAccessor.SetAsync(stepContext.Context, perfilDeUsuario,
                    cancellationToken);
            }

            return await stepContext.EndDialogAsync(exito, cancellationToken);
        }

        private async Task<bool> EsOtpValidoAsync(string texto, ITurnContext context, CancellationToken cancellationToken)
        {
            //Mando a sacar el objeto de la coversacion donde ya guarde el OTP
            DataConversation dataConversation =
                await _botStateService.DataConversationAccessor.GetAsync(context,
                    () => new DataConversation(), cancellationToken);

            bool valid = texto == dataConversation.OTP;

            return valid;
        }
    }
}
