using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotFrameworkSample.Model
{
    public class DataConversation
    {
        public DataConversation()
        {
           
        }

        /// <summary>
        /// OTP generado para el usuario en este momento
        /// </summary>
        public string OTP { get; set; }
    }
}
