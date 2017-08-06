using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Infrastructure;

namespace Mite.DAL.Repositories
{
    public class SocialLinksRepository : Repository<SocialLinks>
    {
        public SocialLinksRepository(AppDbContext db) : base(db)
        {
            TableName = "SocialLinks";
        }
    }
}