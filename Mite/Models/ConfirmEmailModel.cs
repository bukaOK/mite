using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Mite.Models
{
    public class EmailSettingsModel
    {
        public string Email { get; set; }
        public bool Confirmed { get; set; }
        public bool EmailConfirmationSended { get; set; }
    }
}