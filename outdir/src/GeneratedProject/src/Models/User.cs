namespace Generated.Models
{
using System.Text.Json.Serialization;
public class User
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

public string? username
{
get;
set;
}

public string? firstName
{
get;
set;
}

public string? lastName
{
get;
set;
}

public string? email
{
get;
set;
}

public string? password
{
get;
set;
}

public string? phone
{
get;
set;
}

public int? userStatus
{
get;
set;
}

}

}
