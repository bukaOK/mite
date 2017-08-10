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
using Mite.DAL.Infrastructure;
using System.Data.Entity;

namespace Mite.DAL.Repositories
{
    public class CashOperationsRepository : Repository<CashOperation>
    {
        public CashOperationsRepository(AppDbContext db) : base(db)
        {
        }
        /// <summary>
        /// Получаем список денежных операций по типу операции и Id владельца
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="withFromUser">Нужно ли присоединять таблицы пользователей-отправителей</param>
        /// <returns></returns>
        public async Task<IEnumerable<CashOperation>> GetByOperationTypeAsync(string userId, CashOperationTypes type, bool withFromUser = false)
        {
            var operations = Table.Where(x => x.ToId == userId && x.OperationType == type);
            if (withFromUser)
            {
                operations = operations.Include(x => x.From);
            }
            return await operations.ToListAsync();
        }
        public IEnumerable<CashOperation> GetByOperationType(string userId, CashOperationTypes type, bool withFromUser = false)
        {
            var operations = Table.Where(x => x.ToId == userId && x.OperationType == type);
            if (withFromUser)
            {
                operations = operations.Include(x => x.From);
            }
            return operations.ToList();
        }
        /// <summary>
        /// Возвращает все денежные операции пользователя(не включая операции ввода-вывода)
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns></returns>
        public async Task<IEnumerable<CashOperation>> GetListAsync(string userId)
        {
            var operations = await Table.Where(x => x.FromId == userId || x.ToId == userId).ToListAsync();
            return operations;
        }
    }
}