
using System.ComponentModel.DataAnnotations;

public class ConnectionStringOptions
{
    [Required]
    public string DefaultConnection { get; set; } = "";
}