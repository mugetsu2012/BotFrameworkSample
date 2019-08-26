using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFrameworkSample.Model
{
    /// <summary>
    /// Objeto que me ayudara a ir guardando los numeros validos del usuario
    /// </summary>
    public class NumeroValido
    {
        /// <summary>
        /// Numero que previamente ya valide
        /// </summary>
        public string Numero { get; set; }

        /// <summary>
        /// Fecha en la que ya no es valido este numero
        /// </summary>
        public DateTime FechaVencimiento { get; set; }
    }
}
