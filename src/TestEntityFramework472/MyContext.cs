using System.Data.Entity;

namespace TestEntityFramework472
{
    [DbConfigurationType(typeof(MyDbConfiguration))]
    class MyContext : DbContext
    {
        public MyContext()
            : base("name=MyContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("MySchema");
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<Blog> Blogs { get; set; }
    }
}
