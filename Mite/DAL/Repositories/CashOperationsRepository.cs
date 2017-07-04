using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Threading.Tasks;
using Mite.Enums;
using Dapper;

namespace Mite.DAL.Repositories
{
    public class CashOperationsRepository : Repository<CashOperation>
    {
        public CashOperationsRepository(IDbConnection db) : base(db)
        {
        }
        /// <summary>
        /// Получаем список денежных операций по типу операции и Id владельца
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="withFromUser">Нужно ли присоединять таблицы пользователей-отправителей</param>
        /// <returns></returns>
        public Task<IEnumerable<CashOperation>> GetByOperationTypeAsync(string userId, CashOperationTypes type, bool withFromUser = false)
        {
            var operationType = (byte)type;
            var query = "select * from dbo.CashOperations ";
            if (withFromUser)
            {
                query += "inner join dbo.AspNetUsers on FromId=dbo.AspNetUsers.Id ";
            }
            query += "where ToId=@userId and OperationType=@operationType";
            if (withFromUser)
            {
                return Db.QueryAsync<CashOperation, User, CashOperation>(query, (cash, user) =>
                {
                    cash.From = user;
                    return cash;
                }, new { operationType, userId });
            }
            return Db.QueryAsync<CashOperation>(query, new { operationType, userId });
        }
        public IEnumerable<CashOperation> GetByOperationType(string userId, CashOperationTypes type, bool withFromUser = false)
        {
            var operationType = (byte)type;
            var query = "select * from dbo.CashOperations ";
            if (withFromUser)
            {
                query += "inner join dbo.AspNetUsers on FromId=dbo.AspNetUsers.Id ";
            }
            query += "where ToId=@userId and OperationType=@operationType";
            if (withFromUser)
            {
                return Db.Query<CashOperation, User, CashOperation>(query, (cash, user) =>
                {
                    cash.From = user;
                    return cash;
                }, new { operationType, userId });
            }
            return Db.Query<CashOperation>(query, new { operationType, userId });
        }
        /// <summary>
        /// Возвращает все денежные операции пользователя(не включая операции ввода-вывода)
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        public Task<IEnumerable<CashOperation>> GetListAsync(string userId)
        {
            var query = "select * from dbo.CashOperations where FromId=@userId or ToId=@userId";
            return Db.QueryAsync<CashOperation>(query, new { userId });
        }
    }
}