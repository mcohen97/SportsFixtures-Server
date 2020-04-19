using Microsoft.EntityFrameworkCore;


namespace ObligatorioDA2.Services.Logging
{
    public class LoggingContext: DbContext
    {
        public virtual DbSet<LogInfoEntity> Logs { get; set; }

        public LoggingContext(DbContextOptions<LoggingContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LogInfoEntity>().HasKey(li => li.Id);
            modelBuilder.Entity<LogInfoEntity>().Property(u => u.Id).ValueGeneratedOnAdd();
        }
    }
}
