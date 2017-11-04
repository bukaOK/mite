using Microsoft.AspNet.Identity;
using Mite.DAL.Infrastructure;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mite.BLL.Core
{
    public interface IDataService { }
    public abstract class DataService : IDataService
    {
        protected readonly IUnitOfWork Database;
        protected readonly ILogger logger;
        protected DataServiceResult Success => DataServiceResult.Success();

        public DataService(IUnitOfWork database, ILogger logger)
        {
            Database = database;
            this.logger = logger;
        }

        protected DataServiceResult CommonError(string message, Exception e)
        {
            logger.Error($"{message}: {e.Message}");
            return DataServiceResult.Failed(message);
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
        private DataServiceResult(bool success, object data, IEnumerable<string> errors)
        {
            Succeeded = success;
            ResultData = data;
            Errors = errors;
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
            return new DataServiceResult(false, data, errors);
        }
        public static DataServiceResult Failed(object data, IEnumerable<string> errors)
        {
            return new DataServiceResult(false, data, errors);
        }
    }
}