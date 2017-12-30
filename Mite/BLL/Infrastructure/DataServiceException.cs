using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.BLL.Infrastructure
{
    public class DataServiceException : Exception
    {
        public DataServiceException() : base() { }
        public DataServiceException(string message) : base(message) { }
    }
}