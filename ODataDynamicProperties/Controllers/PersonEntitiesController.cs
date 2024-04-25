using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataDynamicProperties.Data;

namespace ODataDynamicProperties.Controllers;

public class PersonEntitiesController : ODataController
{
    private readonly MyDbContext _DbContext;


    public PersonEntitiesController(MyDbContext dbContext)
    {
        _DbContext = dbContext;
    }

    // PersonEntity is exposed by the edm modell just for the sake of analyis
    [EnableQuery]
    public IQueryable<PersonEntity> Get()
    {
        IQueryable<PersonEntity> personsQuery = _DbContext.Persons;
        return personsQuery;
    }
}