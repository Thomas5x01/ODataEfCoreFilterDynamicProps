using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using ODataDynamicProperties.Data;
using ODataDynamicProperties.Dto;

namespace ODataDynamicProperties.Controllers;

public class PersonDtosController : ODataController
{
    private readonly MyDbContext _DbContext;


    public PersonDtosController(MyDbContext dbContext)
    {
        _DbContext = dbContext;
    }
    
    // PersonDto is the type that should be exposed in our real application
    [EnableQuery]
    public IQueryable Get([FromServices ] ODataQueryOptions<PersonDto> queryOptions)
    {
        // preparing the query by describing the projection
        // from entity to dto,
        // and also from the type with indexer property to the type that contains a property of type IDictionary<string, object>
        IQueryable<PersonDto> personDtosQuery = _DbContext.Persons
            .Select(entity => new PersonDto()
                {
                    Id = entity.Id,
                    Name = entity.Name,
                    Fields = new Dictionary<string, object>
                    {
                        { "Birthday", entity["Birthday"] },
                        { "LikesFootball", entity["LikesFootball"] }
                    }
                }
            );

        // ============================================

        // a small test to narrow down the problem:
        // what should an expression look like, that can be translated by 
        // Microsoft.EntityFrameworkCore.Query.RelationalSqlTranslatingExpressionVisitor

        // this where clause works fine
        List<PersonEntity> personEntityList = _DbContext.Persons
            .Where(x => EF.Property<bool>(x, "LikesFootball") == true)
            .ToList();


        // is it possible to write a query by hand that can be translated ?
        // each of these where clauses fails
        List<PersonDto> personDtoList = personDtosQuery
                //.Where(x => x.Fields.ContainsKey("Birthday"))
                //.Wherex => EF.Property<bool>(x, "LikesFootball") == true)
                //.Where(x => EF.Property<bool>(x.Fields, "LikesFootball") == true)
                .ToList();
        // ============================================


        // If we just return the query, an exception that might occure
        // is caugth by the Odata / Aspnetcore infrastructure.
        // This way you can make it pop up while debugging:
        if (queryOptions.Filter != null)
        {
            IQueryable? filteredQuery = queryOptions.ApplyTo(personDtosQuery);
            // starting the iteration of the untyped queryable will cause the filter / predicate to be run against EfCore
            // and an exception will pop up, in case the filter contained access to one of the dynamic properties
            //System.ArgumentException
            // HResult=0x80070057
            // Message = Method 'System.Object get_Item(System.String)' declared on type 'System.Collections.Generic.Dictionary`2[System.String,System.Object]'
            //          cannot be called with instance of type 'System.Object'
            // Source = System.Linq.Expressions
            // Stack:
            //  at System.Linq.Expressions.Expression.ValidateCallInstanceType(Type instanceType, MethodInfo method)

            foreach (object item in filteredQuery)
            { }
        }

        return personDtosQuery;
    }
}