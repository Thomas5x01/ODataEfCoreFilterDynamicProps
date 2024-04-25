using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using ODataDynamicProperties.Data;
using ODataDynamicProperties.Dto;

namespace ODataDynamicProperties;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var mvcBuilder = builder.Services.AddControllers();
        AddOData(mvcBuilder);

        builder.Services.AddDbContext<MyDbContext>(opt =>
        {
            // change the connection string to make it fit with your environment
            opt.UseSqlServer("Server=localhost;Database=DynamicProperties;User Id=sa;Password=???;TrustServerCertificate=True;");
            opt.LogTo(Console.WriteLine);
        });

        var app = builder.Build();
        await RecreateAndSeedDb(app.Services);

        // Configure the HTTP request pipeline.
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }

    private static void AddOData(IMvcBuilder mvcBuilder)
    {
        ODataConventionModelBuilder modelBuilder = new();
            
        // PersonDto is the type that should be exposed in our real application
        modelBuilder.EntitySet<PersonDto>("PersonDtos");
        // PersonEntity is exposed by the edm modell just for the sake of analyis
        modelBuilder.EntitySet<PersonEntity>("PersonEntities");

        NameResolverOptions nameResolverOptions =
            NameResolverOptions.ProcessDataMemberAttributePropertyNames | NameResolverOptions.ProcessReflectedPropertyNames;

        modelBuilder.EnableLowerCamelCase(nameResolverOptions);

        mvcBuilder.AddOData(
            options =>
            {
                options.EnableQueryFeatures();
                options.AddRouteComponents("", modelBuilder.GetEdmModel());
            });
    }

    private static async Task RecreateAndSeedDb(IServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        MyDbContext dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            
        Console.WriteLine("Starting EnsureDeletedAsync");
        await dbContext.Database.EnsureDeletedAsync().ConfigureAwait(false);
        Console.WriteLine("Starting EnsureCreatedAsync");
        await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);

        Console.WriteLine("Starting seed data");
        PersonEntity personMax = new PersonEntity();
        personMax.Name = "Max";
        personMax["Birthday"] = DateTime.Parse("01.01.2000");
        personMax["LikesFootball"] = false;
        dbContext.Add(personMax);

        PersonEntity personLisa = new PersonEntity();
        personLisa.Name = "Lisa";
        personLisa["Birthday"] = DateTime.Parse("01.02.2000");
        personLisa["LikesFootball"] = true;
        dbContext.Add(personLisa);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}