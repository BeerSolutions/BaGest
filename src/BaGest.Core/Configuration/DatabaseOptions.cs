using System.ComponentModel.DataAnnotations;

namespace BaGest.Core;

public class DatabaseOptions
{
    public string Type { get; set; }

    [Required]
    public string ConnectionString { get; set; }
}
