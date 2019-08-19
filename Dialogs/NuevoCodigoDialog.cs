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
    public class NuevoCodigoDialog: ComponentDialog
    {
        private readonly BotStateService _botStateService;

        public NuevoCodigoDialog(string dialogId, BotStateService botStateService): base(dialogId)
        {
            _botStateService = botStateService;

            InicializarCascada();
        }

        private void InicializarCascada()
        {
            WaterfallStep[] steps = new WaterfallStep[]
            {
                NotificarCodigoReenviadoStepAsync,
                ValidarOtpStepAsync,
                FinalizarStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(NuevoCodigoDialog)}.mainFlow", steps));
            AddDialog(new TextPrompt($"{nameof(NuevoCodigoDialog)}.otp"));

            InitialDialogId = $"{nameof(NuevoCodigoDialog)}.mainFlow";

        }

        private async Task<DialogTurnResult> NotificarCodigoReenviadoStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Mando a generar el OTP 
            string otpFake = RandomString(8);

            //Mando a sacar el DataConversation para setearle el OTP

            DataConversation dataConversation =
                await _botStateService.DataConversationAccessor.GetAsync(stepContext.Context,
                    () => new DataConversation(), cancellationToken);

            dataConversation.OTP = otpFake;

            //Mando a setear al state la nueva data
            await _botStateService.DataConversationAccessor.SetAsync(stepContext.Context, dataConversation, cancellationToken);

            //Le solicitamos al usuario que nos diga el OTP

            return await stepContext.PromptAsync($"{nameof(NuevoCodigoDialog)}.otp", new PromptOptions()
            {
                Prompt = MessageFactory.Text(
                    $"Se le reenvio el codigo a su telefono, favor ingresarlo (pssst, el codigo es: {otpFake})")
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

        private string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
