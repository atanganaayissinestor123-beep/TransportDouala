using SQLite;

namespace TransportDouala.Data;

[Table("users")]
public class UserEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    public string FullName { get; set; } = "";
    [Unique] public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Salt { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

[Table("lines")]
public class LineEntity
{
    [PrimaryKey] public int Id { get; set; }
    public string Type { get; set; } = "";
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int BaseFare { get; set; }
}

[Table("line_stops")]
public class LineStopEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int LineId { get; set; }
    public int Seq { get; set; }
    public string Name { get; set; } = "";
    public double Lat { get; set; }
    public double Lon { get; set; }
    public int Minutes { get; set; }
}

[Table("fare_reports")]
public class FareReportEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int UserId { get; set; }
    [Indexed] public int LineId { get; set; }
    public string Direction { get; set; } = "";
    public int Fare { get; set; }
    public DateTime Date { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

[Table("favorite_lines")]
public class FavoriteLineEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int UserId { get; set; }
    public int LineId { get; set; }
}

[Table("favorite_routes")]
public class FavoriteRouteEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int UserId { get; set; }
    public string Key { get; set; } = "";
    public int LineId { get; set; }
    public string FromLabel { get; set; } = "";
    public string ToLabel { get; set; } = "";
    public string FromStop { get; set; } = "";
    public string ToStop { get; set; } = "";
    public int Minutes { get; set; }
    public int StopCount { get; set; }
}

[Table("recent_searches")]
public class RecentSearchEntity
{
    [PrimaryKey, AutoIncrement] public int Id { get; set; }
    [Indexed] public int UserId { get; set; }
    public string Name { get; set; } = "";
    public DateTime SearchedAt { get; set; }
}
