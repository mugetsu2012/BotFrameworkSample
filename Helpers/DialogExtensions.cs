using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BotFrameworkSample.Helpers
{
    public static class DialogExtensions
    {
        public static async Task Run(this Dialog dialog, ITurnContext turnContext, IStatePropertyAccessor<DialogState> accessor, CancellationToken cancellationToken)
        {
            //Creo un dialog set a partir del accesor de state
            DialogSet dialogSet = new DialogSet(accessor);

            //Agrego el dialogo actual al set
            dialogSet.Add(dialog);

            //Creo un dialo context a parti del turn
            DialogContext dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);

            //Obtengo el resultado de continuar con el dialogo actual
            DialogTurnResult result = await dialogContext.ContinueDialogAsync(cancellationToken);

            //Si no hay ningun dialogo corriendo, iniciamos el actual
            if (result.Status == DialogTurnStatus.Empty)
            {
                await dialogContext.BeginDialogAsync(dialog.Id, null, cancellationToken);
            }
        }
    }
}
