using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace TestEntityFramework472
{
    [DbConfigurationType(typeof(MyDbConfiguration))]
    class MyContext : DbContext
    {
        public MyContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {            
            var objectContext = (this as IObjectContextAdapter).ObjectContext;
            objectContext.DatabaseExists();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("MySchema");
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<Blog> Blogs { get; set; }
    }
}
