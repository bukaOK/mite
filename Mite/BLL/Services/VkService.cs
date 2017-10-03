using Mite.BLL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Mite.BLL.Services
{
    public interface IVkService : IDataService
    {
        Task<DataServiceResult> FindVkReposts()
    }
    public class VkService
    {

    }
}