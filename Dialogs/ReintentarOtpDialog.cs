using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotFrameworkSample.Model;
using BotFrameworkSample.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace BotFrameworkSample.Dialogs
{
    /// <summary>
    /// Este dialogo sirve para preguntar al usuario si quiere reintentar el codigo OTP
    /// o quiere que se le envie otro. Este dialogo actua como el padre de los dialogos Reintento y nuevo
    /// </summary>
    public class ReintentarOtpDialog: ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly int _minutosNumeroValido;

        public ReintentarOtpDialog(string dialogId, BotStateService botStateService, int minutosNumeroValido): base(dialogId)
        {
            _botStateService = botStateService;
            _minutosNumeroValido = minutosNumeroValido;

            InicializarCascada();
        }

        private void InicializarCascada()
        {
            WaterfallStep[] steps = new WaterfallStep[]
            {
                PreguntarReintentoStepAsync,
                ProcesarRespuestaReintentoStepAsync,
                ValidarOtpStepAsync,
                FinalizarStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(ReintentarOtpDialog)}.mainFlow", steps));
            AddDialog(new TextPrompt($"{nameof(ReintentarOtpDialog)}.otp"));
            AddDialog(new ChoicePrompt($"{nameof(ReintentarOtpDialog)}.pregunta"));

            InitialDialogId = $"{nameof(ReintentarOtpDialog)}.mainFlow";

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
            return await stepContext.PromptAsync($"{nameof(ReintentarOtpDialog)}.pregunta", new PromptOptions()
            {
                Prompt = MessageFactory.Text("El codigo ingresado no es valido. ¿Desea reintentar o que se le envie un nuevo codigo? "),
                Choices = ChoiceFactory.ToChoices(new List<string>() { "Reintentar", "Nuevo"})
            }, cancellationToken);
        }

        /// <summary>
        /// En este paso leemos lo que dijo sobre re intentar o sobre querer un codigo nuevo
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ProcesarRespuestaReintentoStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Extramos el dato de la opcion anterior
            string eleccion = ((FoundChoice)stepContext.Result).Value;

            DataConversation dataConversation =
                await _botStateService.DataConversationAccessor.GetAsync(stepContext.Context,
                    () => new DataConversation(), cancellationToken);

            //Verificamos que fue lo que eligio
            if (eleccion == "Reintentar")
            {
                //Le solicitamos al usuario que nos diga el OTP

                return await stepContext.PromptAsync($"{nameof(ReintentarOtpDialog)}.otp", new PromptOptions()
                {
                    Prompt = MessageFactory.Text(
                        $"Muy bien, ingrese el codigo enviado a su telefono (pssst, el codigo es: {dataConversation.OTP})")
                }, cancellationToken);


                //Si pidio reintentar, levanto el dialogo de re intento
                //return await stepContext.BeginDialogAsync($"{nameof(ReintentarOtpDialog)}.reintentar", null,
                //    cancellationToken);
            }
            else
            {

                //Mando a generar el OTP 
                string otpFake = RandomString(8);

                //Seteo el OTP
                dataConversation.OTP = otpFake;

                //Mando a setear al state la nueva data
                await _botStateService.DataConversationAccessor.SetAsync(stepContext.Context, dataConversation, cancellationToken);

                //Le solicitamos al usuario que nos diga el OTP

                return await stepContext.PromptAsync($"{nameof(ReintentarOtpDialog)}.otp", new PromptOptions()
                {
                    Prompt = MessageFactory.Text(
                        $"Se le reenvio el codigo a su telefono, favor ingresarlo (pssst, el codigo es: {otpFake})")
                }, cancellationToken);

                //Si dijo que quiere un codigo nuevo, se le manda al dialogo de nuevo codigo
                //return await stepContext.BeginDialogAsync($"{nameof(ReintentarOtpDialog)}.nuevoCodigo", null,
                //    cancellationToken);

            }
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
            return await stepContext.NextAsync(esOtpValido, cancellationToken);
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
                //Le seteo el OTP al fulano

                // Sacamos al usuario
                PerfilDeUsuario perfilDeUsuario = await _botStateService.PerfilDeUsuarioAccessor.GetAsync(stepContext.Context,
                    () => new PerfilDeUsuario(), cancellationToken);

                perfilDeUsuario.NumeroOTP = (string)stepContext.Values["otp"];

                perfilDeUsuario.AgregarNumeroValido(new NumeroValido()
                {
                    Numero = perfilDeUsuario.NumeroTelefono,
                    FechaVencimiento = DateTime.Now.AddMinutes(_minutosNumeroValido)
                });

                await _botStateService.PerfilDeUsuarioAccessor.SetAsync(stepContext.Context, perfilDeUsuario,
                    cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync($"{nameof(ReintentarOtpDialog)}.mainFlow", null, cancellationToken);
            }
            
        }

        private string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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
