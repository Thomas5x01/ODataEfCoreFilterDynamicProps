using Microsoft.EntityFrameworkCore;

namespace ODataDynamicProperties.Data;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions optionsBuilder) : 
        base(optionsBuilder)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var typeBuilder = modelBuilder.Entity<PersonEntity>();
        typeBuilder.HasKey(x => x.Id);
        typeBuilder.ToTable("Persons");

        // configuring the indexer properties, they represent the dynamic part of the modell
        // https://learn.microsoft.com/en-Us/ef/core/modeling/shadow-properties#configuring-indexer-properties
        typeBuilder.IndexerProperty<DateTime?>("Birthday").IsRequired(false);
        typeBuilder.IndexerProperty<bool?>("LikesFootball").IsRequired(false);
    }

    public DbSet<PersonEntity> Persons { get; set; }
}