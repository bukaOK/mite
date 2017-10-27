using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.DAL.Initializers.Core
{
    internal interface IInitializer
    {
        bool Initialized { get; }
        void Initialize();
    }
}
