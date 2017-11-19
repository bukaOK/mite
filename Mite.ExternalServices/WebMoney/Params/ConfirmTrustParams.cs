using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.WebMoney.Params
{
    public class ConfirmTrustParams
    {
        /// <summary>
        /// Номер запроса, хранившийся в сессии
        /// </summary>
        public string PurseId { get; set; }
        /// <summary>
        /// Смс код подтверждения
        /// </summary>
        public string ConfirmCode { get; set; }
    }
}
