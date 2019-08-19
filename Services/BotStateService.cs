using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotFrameworkSample.Model;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BotFrameworkSample.Services
{
    public class BotStateService
    {
        #region Variables

        /// <summary>
        /// Estado para guardar informacion de la conversacion, ej: Dialogos y otros estados
        /// </summary>
        public ConversationState ConversationState { get; }

        /// <summary>
        /// Estado para guardar informacion del usuario, en este caso un Perfil de usuario
        /// </summary>
        public UserState UserState { get; }

        #endregion

        #region Ids

        /// <summary>
        /// Id unico para este state service con este objeto de perfil
        /// </summary>
        public string IdPerfilUsuario { get; } = $"{nameof(BotStateService)}.PerfilDeUsuario";

        /// <summary>
        /// Id unico para el objeto de datos de conversacion
        /// </summary>
        public string IdDataConversation { get; } = $"{nameof(BotStateService)}.DataConversation";

        /// <summary>
        /// Id unico para este state service para todos los dialogos generadoss
        /// </summary>
        public string DialogStateId { get; } = $"{nameof(BotStateService)}.DialogState";

        #endregion

        #region Accessors

        public IStatePropertyAccessor<PerfilDeUsuario> PerfilDeUsuarioAccessor { get; set; }

        public IStatePropertyAccessor<DataConversation> DataConversationAccessor { get; set; }

        public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }


        #endregion

        public BotStateService(UserState userState, ConversationState conversationState)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));

            //Inicializamos los accessors 
            InitializeAccessors();

        }

        /// <summary>
        /// Metodo que me crea los accessors con las variables definidas por mi para cada estado
        /// </summary>
        public void InitializeAccessors()
        {
            //Creamos el accesor de perfil de usaurio con el id previamente definido
            PerfilDeUsuarioAccessor = UserState.CreateProperty<PerfilDeUsuario>(IdPerfilUsuario);

            //Creamos el accessor del dialogo usando el conversation state y su id de dialogo
            DialogStateAccessor = ConversationState.CreateProperty<DialogState>(DialogStateId);

            DataConversationAccessor = ConversationState.CreateProperty<DataConversation>(IdDataConversation);
        }
    }
}
