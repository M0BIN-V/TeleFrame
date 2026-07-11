namespace Sample;

public class AdminsService
{
    // fake admins 
    public List<Admin> Admins { get; set; } =
    [
        new() { Id = 123456789, Username = "admin1" },
        new() { Id = 987654321, Username = "admin2" }
    ];
}

public class Admin
{
    public long Id { get; set; }
    public required string Username { get; set; }
}