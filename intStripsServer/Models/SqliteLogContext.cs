using Microsoft.EntityFrameworkCore;

namespace intStripsServer.Models;

public class SqliteLogContext : DbContext
{
    private readonly string _dbPath;
    
    public DbSet<FieldUpdate> FieldUpdates { get; set; }
    
    public SqliteLogContext(DbContextOptions<SqliteLogContext> options) : base(options)
    { }
}

public class FieldUpdate
{
    public string Cid { get; set; }
    public string Callsign { get; set; }
    public string Field { get; set; }
    public string? Update { get; set; }
}