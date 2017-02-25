using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomizeMigrationTable
{   
    public class MySpecialDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
    }
}
