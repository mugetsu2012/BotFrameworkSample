using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BotFrameworkSample.Model;
using BotFrameworkSample.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace BotFrameworkSample.Dialogs
{
    public class ObtenerReporteDialog: ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly int _minutosVencerNumero;

        public ObtenerReporteDialog(string dialogId, BotStateService botStateService, int minutosVencerNumero):base(dialogId)
        {
            _botStateService = botStateService ?? throw new ArgumentNullException(nameof(botStateService));
            _minutosVencerNumero = minutosVencerNumero;

            //Mandamos a confifurar este dialogo
            InicializarCascada();
        }

        /// <summary>
        /// Metodo que me inicializa el dialogo en cascada
        /// </summary>
        private void InicializarCascada()
        {
            //Creamos los pasos del Waterfall
            WaterfallStep[] steps = new WaterfallStep[]
            {
                OpcionesStepAsync,
                NumeroTelefonoStepAsync,
                OtpStepAsync,
                ValidarOtpStepAsync,
                ResumenStepAsync
            };

            //Agregamos los dialogos a usar
            AddDialog(new WaterfallDialog($"{nameof(ObtenerReporteDialog)}.mainFlow", steps));
            AddDialog(new ChoicePrompt($"{nameof(ObtenerReporteDialog)}.opcion"));
            AddDialog(new TextPrompt($"{nameof(ObtenerReporteDialog)}.telefono", TelefonoValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(ObtenerReporteDialog)}.otp"));
            AddDialog(new ReintentarOtpDialog($"{nameof(ObtenerReporteDialog)}.preguntarReintento",
                _botStateService, _minutosVencerNumero));

            //Seteamos el main id
            InitialDialogId = $"{nameof(ObtenerReporteDialog)}.mainFlow";

        }

        #region Pasos


        /// <summary>
        /// Primer metodo en ser llamado. Da una explicacion al usuario de que esta pasando
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> OpcionesStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Seteo al usuario la fecha y hora de la consulta
            PerfilDeUsuario perfilDeUsuario = await _botStateService.PerfilDeUsuarioAccessor.GetAsync(stepContext.Context,
                () => new PerfilDeUsuario(), cancellationToken);

            perfilDeUsuario.FechaConsulta = DateTime.Now;

            //Mando a setear al state
            await _botStateService.PerfilDeUsuarioAccessor.SetAsync(stepContext.Context, perfilDeUsuario,
                cancellationToken);

            //Mandaos a dar una bienvenida y pedir una opcion
            return await stepContext.PromptAsync($"{nameof(ObtenerReporteDialog)}.opcion", new PromptOptions()
            {
                Prompt = MessageFactory.Text("Por favor elija una opcion: "),
                Choices = ChoiceFactory.ToChoices(new List<string>() { "Reporte1", "Reporte2", "Reporte3" })
            }, cancellationToken);
        }

        /// <summary>
        /// Metodo en el cual le pido al numero al usuario
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> NumeroTelefonoStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Extramos el dato de la opcion anterior
            stepContext.Values["opcion"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync($"{nameof(ObtenerReporteDialog)}.telefono", new PromptOptions()
            {
                Prompt = MessageFactory.Text("Por favor ingrese su numero de telefono"),
                RetryPrompt =
                    MessageFactory.Text(
                        "El numero de telefono no es valido. Debe ingresar un numero en el formato xxxx-xxxx")
            }, cancellationToken);
        }

        /// <summary>
        /// Metodo en el cual le pido el OTP al usuario
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> OtpStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Extraemos el numero de telefono del paso anterior
            string telefono = (string)stepContext.Result;
            stepContext.Values["telefono"] = telefono;

            DataConversation dataConversation =
                await _botStateService.DataConversationAccessor.GetAsync(stepContext.Context,
                    () => new DataConversation(), cancellationToken);

            PerfilDeUsuario perfilDeUsuario =
                await _botStateService.PerfilDeUsuarioAccessor.GetAsync(stepContext.Context,
                    () => new PerfilDeUsuario(), cancellationToken);


            //Seteo en numero que el usuario ingreso
            perfilDeUsuario.NumeroTelefono = telefono;

            //Si hay un numero valido, lo mando de un solo al reporte sin  pasar por el OTP
            if (perfilDeUsuario.NumerosValidos.Any(y => y.Numero == telefono && y.FechaVencimiento >= DateTime.Now))
            {
                //Marco que el numero actual es valido para que me deje pasar
                perfilDeUsuario.EsNumeroIngresadoValido = true;

                await _botStateService.PerfilDeUsuarioAccessor.SetAsync(stepContext.Context, perfilDeUsuario,
                    cancellationToken);

                return await stepContext.NextAsync("", cancellationToken);
            }
            else
            {
                //Mando a generar el OTP 
                string otpFake = RandomString(8);

                dataConversation.OTP = otpFake;

                //Mando a setear al state la nueva data
                await _botStateService.DataConversationAccessor.SetAsync(stepContext.Context, dataConversation, cancellationToken);

                await _botStateService.PerfilDeUsuarioAccessor.SetAsync(stepContext.Context, perfilDeUsuario,
                    cancellationToken);

                //Le solicitamos al usuario que nos diga el OTP

                return await stepContext.PromptAsync($"{nameof(ObtenerReporteDialog)}.otp", new PromptOptions()
                {
                    Prompt = MessageFactory.Text(
                        $"Por favor ingrese el codigo enviado a su telefono (pssst, el codigo es: {otpFake})")
                }, cancellationToken);
            }

            
        }


        private async Task<DialogTurnResult> ValidarOtpStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {

            //Lo primero es que si el nuero era valido, nos saltamos el proceso

            PerfilDeUsuario perfilDeUsuario = await _botStateService.PerfilDeUsuarioAccessor.GetAsync(stepContext.Context,
                () => new PerfilDeUsuario(), cancellationToken);
            //Si el numero es valido me salto el proceso

            if (perfilDeUsuario.EsNumeroIngresadoValido)
            {
                //Marco la variable de conversacion como false, para resetear el estado
                perfilDeUsuario.EsNumeroIngresadoValido = false;

                //Mando a guardar
                await _botStateService.PerfilDeUsuarioAccessor.SetAsync(stepContext.Context, perfilDeUsuario,
                    cancellationToken);

                //Lo mando al paso final
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                //Entra aca porque su numero no esta en la lista de validados

                string otpIngresado = (string)stepContext.Result;
                //Sacamos el OTP que dio el usuario del paso anterior
                stepContext.Values["otp"] = otpIngresado;

                bool esOtpValido =
                    await EsOtpValidoAsync(otpIngresado, stepContext.Context, cancellationToken);

                //Si el OTP es valido, lo mando al resumen
                if (esOtpValido)
                {
                    //Le seteo el OTP al fulano
                    perfilDeUsuario.NumeroOTP = otpIngresado;

                    perfilDeUsuario.AgregarNumeroValido(new NumeroValido()
                    {
                        Numero = perfilDeUsuario.NumeroTelefono,
                        FechaVencimiento = DateTime.Now.AddMinutes(_minutosVencerNumero)
                    });

                    await _botStateService.PerfilDeUsuarioAccessor.SetAsync(stepContext.Context, perfilDeUsuario,
                        cancellationToken);

                    return await stepContext.NextAsync(null, cancellationToken);
                }
                else
                {
                    //Si el OTP no es valido lo mando a al step de preguntar que quiere hacer
                    return await stepContext.BeginDialogAsync($"{nameof(ObtenerReporteDialog)}.preguntarReintento", null,
                        cancellationToken);
                }
            }

            
        }

        /// <summary>
        /// Metodo final. Aca le doy un resumen y termino el dialogo
        /// </summary>
        /// <param name="stepContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<DialogTurnResult> ResumenStepAsync(WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            //Sacamos al usuario
            PerfilDeUsuario perfilDeUsuario = await _botStateService.PerfilDeUsuarioAccessor.GetAsync(stepContext.Context,
                () => new PerfilDeUsuario(), cancellationToken);

            //Seteo al usuario la data
            perfilDeUsuario.NumeroTelefono = (string)stepContext.Values["telefono"];
            perfilDeUsuario.OpcionElegida = (string)stepContext.Values["opcion"];

            //Muestro el resumen al usuario y su reporte
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Su reporte es: (Aca va lo que queramos darle)"), cancellationToken);

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"La opcion elegida fue: {perfilDeUsuario.OpcionElegida}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"El numero de telefono ingresado fue: {perfilDeUsuario.NumeroTelefono}"),
                cancellationToken);

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text($"El OTP registrado fue: {perfilDeUsuario.NumeroOTP}"), cancellationToken);

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(
                    $"La fecha y hora de su consulta fue: {perfilDeUsuario.FechaConsulta.ToString("g")}"),
                cancellationToken);

            //Mando a guardar el usuario

            await _botStateService.PerfilDeUsuarioAccessor.SetAsync(stepContext.Context, perfilDeUsuario,
                cancellationToken);

            //Termino la conversacion
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);

        }



        #endregion

        #region Validaciones

        private Task<bool> TelefonoValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            bool valid = false;

            //si el prompt reconocio el texto
            if (promptContext.Recognized.Succeeded)
            {
                //Valido si cumple el REGEX de 4-4  
                valid = Regex.Match(promptContext.Recognized.Value, @"\d{4}?[\s.-]?\d{4}$").Success;
            }

            return Task.FromResult(valid);
        }

        private async Task<bool> OtpValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            bool valid = false;

            //Si el prompt reconocio la entrada 
            if (promptContext.Recognized.Succeeded)
            {
                //Mando a sacar el objeto de la coversacion donde ya guarde el OTP
                DataConversation dataConversation =
                    await _botStateService.DataConversationAccessor.GetAsync(promptContext.Context,
                        () => new DataConversation(), cancellationToken);

                valid = promptContext.Recognized.Value == dataConversation.OTP;

                if (!valid)
                {
                    await promptContext.Context.SendActivityAsync(
                        MessageFactory.Text(
                            $"El codigo OTP no es valido. (Recuerda que el codigo es {dataConversation.OTP})"),
                        cancellationToken);
                }
            }
            return valid;
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

        #endregion


        #region Utilidades


        private string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #endregion
    }
}
