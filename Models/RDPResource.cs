using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KSol.RDPGateway.Models;
public class RDPResource
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string? ResourceIdentifier { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    List<RDPResourceUserAuthorization>? UserAuthorizations { get; set; }
}
