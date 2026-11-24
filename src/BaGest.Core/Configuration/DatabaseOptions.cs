using System.ComponentModel.DataAnnotations;

namespace BaGest.Core;

public class DatabaseOptions
{
    public string Type { get; set; }

    [Required]
    public string ConnectionString { get; set; }

    /// <summary>
    /// The SQLite journal mode (<c>DELETE</c>, <c>TRUNCATE</c>, <c>PERSIST</c>, <c>MEMORY</c>, <c>WAL</c> or <c>OFF</c>)
    /// to apply to the database on startup. Only used by the <c>Sqlite</c> database type.
    /// If not set, the journal mode of the database file is left unchanged.
    /// </summary>
    public string JournalMode { get; set; }
}
