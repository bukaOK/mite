using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mite.ExternalServices.WebMoney.Business
{
    public class Phone
    {
        private readonly string phone;

        public Phone(string phone)
        {
            this.phone = Regex.Replace(phone, "[^0-9]", "");
        }
        public override string ToString()
        {
            return phone;
        }
    }
}
