namespace Generated.Models
{
using System.Text.Json.Serialization;
public class Order
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

public int? petId
{
get;
set;
}

public int? quantity
{
get;
set;
}

public string? shipDate
{
get;
set;
}

public string? status
{
get;
set;
}

public bool? complete
{
get;
set;
}

}

}
