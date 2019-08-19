using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFrameworkSample.Model
{
    /// <summary>
    /// Clase que nos sirve para almacenar la info del usuario
    /// </summary>
    public class PerfilDeUsuario
    {
        /// <summary>
        /// Almacenamo lo que queria el usuario
        /// </summary>
        public string OpcionElegida { get; set; }

        /// <summary>
        /// Guardamos el telefono del usuario
        /// </summary>
        public string NumeroTelefono { get; set; }

        /// <summary>
        /// Variable que nos servira a nosotros paara saber cuando se realizo esta consulta
        /// </summary>
        /// <remarks>Esto se llana al iniciar la conversaion</remarks>
        public DateTime FechaConsulta { get; set; }

        /// <summary>
        /// Guardamos el OTP del usuario
        /// </summary>
        public string NumeroOTP { get; set; }
    }
}
