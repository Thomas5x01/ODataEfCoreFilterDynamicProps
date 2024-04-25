namespace ODataDynamicProperties.Dto;

public class PersonDto
{
    public long Id { get; set; }
    public string Name { get; set; }

    // OData recognizes PersonDto as an open type because it owns Dictionary<string, object> Fields
    private Dictionary<string, object> _Fields = new Dictionary<string, object>();
    public Dictionary<string, object> Fields
    {
        get { return _Fields; }
        set { _Fields = value; }
    }
}