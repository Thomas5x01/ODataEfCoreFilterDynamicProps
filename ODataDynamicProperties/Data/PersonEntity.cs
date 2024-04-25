namespace ODataDynamicProperties.Data;

public class PersonEntity
{
    public long Id { get; set; }
    public string Name { get; set; }

    // EfCore expects a type that provides an indexer property 
    // https://learn.microsoft.com/en-Us/ef/core/modeling/shadow-properties#configuring-indexer-properties

    // OData does not recognize PersonEntity as an open type - see $metadata 
    private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
    public object this[string key]
    {
        get => _data[key];
        set => _data[key] = value;
    }
}