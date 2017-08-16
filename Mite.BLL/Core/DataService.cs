using Microsoft.AspNet.Identity;
using Mite.DAL.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Mite.BLL.Core
{
    public interface IDataService { }
    public abstract class DataService : IDataService
    {
        protected readonly IUnitOfWork Database;

        protected DataService(IUnitOfWork database)
        {
            Database = database;
        }
    }
    public class DataServiceResult
    {
        private DataServiceResult(IEnumerable<string> errors)
        {
            Succeeded = false;
            Errors = errors;
        }
        private DataServiceResult(bool success, object data)
        {
            Succeeded = success;
            ResultData = data;
        }

        public object ResultData { get; }
        public bool Succeeded { get; }
        public IEnumerable<string> Errors { get; }

        public static DataServiceResult Success()
        {
            return Success(null);
        }
        public static DataServiceResult Success(object data)
        {
            return new DataServiceResult(true, data);
        }
        public static DataServiceResult Failed(params string[] errors)
        {
            return Failed(errors.ToList());
        }
        public static DataServiceResult Failed(IEnumerable<string> errors)
        {
            return new DataServiceResult(errors);
        }
        public static DataServiceResult Failed(object data, params string[] errors)
        {
            return new DataServiceResult(false, data);
        }
        public static DataServiceResult Failed(object data, IEnumerable<string> errors)
        {
            return new DataServiceResult(false, data);
        }
    }
}