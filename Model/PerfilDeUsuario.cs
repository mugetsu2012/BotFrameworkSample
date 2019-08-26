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
        public PerfilDeUsuario()
        {
            NumerosValidos = new List<NumeroValido>();
            EsNumeroIngresadoValido = false;
        }


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



        /// <summary>
        /// Indica que este numero se encuentra dentro de la lista de datos y por lo tanto no hay que validar
        /// </summary>
        public bool EsNumeroIngresadoValido { get; set; }

        /// <summary>
        /// Lista de numeros que ya son validos por haber pasado el proceso OTP
        /// </summary>
        public List<NumeroValido> NumerosValidos { get; set; }

        public void AgregarNumeroValido(NumeroValido numeroValido)
        {
            //Primero veo que si ya tengo este numero nomas lo sobreeescribo
            if (NumerosValidos.Any(y => y.Numero == numeroValido.Numero))
            {
                NumerosValidos.First(t => t.Numero == numeroValido.Numero).FechaVencimiento =
                    numeroValido.FechaVencimiento;
            }
            else
            {
                NumerosValidos.Add(numeroValido);
            }
        }
    }
}
