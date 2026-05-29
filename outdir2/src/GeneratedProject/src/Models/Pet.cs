namespace Generated.Models
{
usingSystem.Text.Json.Serialization;
public class Pet
{
/// <example>
/// 10
/// </example>
[Key]
public int? id
{
get;
set;
}

/// <example>
/// doggie
/// </example>
[Required]
public string name
{
get;
set;
}

public object category
{
get;
set;
}

/// <xml-wrapped>
/// true
/// </xml-wrapped>
[Required]
public object photoUrls
{
get;
set;
}

public object tags
{
get;
set;
}

public string? status
{
get;
set;
}

}

}
