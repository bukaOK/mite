using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.ExternalServices.VkApi.Core
{
    internal class VkParamAttribute : Attribute
    {
        public string Name { get; set; }

        public VkParamAttribute(string name)
        {
            Name = name;
        }
    }
}
