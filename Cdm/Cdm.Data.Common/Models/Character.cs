namespace Cdm.Data.Common.Models;

using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;
public abstract class ACharacter
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public string Picture { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? Background { get; set; } = string.Empty;

    public int Life { get; set; }

    public int Leveling { get; set; }

    // Nouveau : Support pour NPCs
    public bool IsNpc { get; set; } = false;

    // Pour les NPCs : dans quel chapitre ils apparaissent
    public int? ChapterId { get; set; }

    [ForeignKey(nameof(ChapterId))]
    public virtual Chapter? Chapter { get; set; }

    // Tags pour organisation et intégration combat (JSON)
    [Column(TypeName = "nvarchar(max)")]
    public string? Tags { get; set; } // JSON array: ["monster", "humanoid", "beast", "official-dnd"]

    // Indicateur si hostile (pour les combats) - uniquement NPCs
    public bool IsHostile { get; set; } = false;

    // Indique si c'est un personnage système (fourni par défaut) ou créé par l'utilisateur
    public bool IsSystemCharacter { get; set; } = false;

    // Type de jeu pour ce personnage
    [Required]
    public GameType GameType { get; set; } = GameType.Generic;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties pour les NPCs
    public virtual ICollection<ContentBlock> ContentBlocks { get; set; } = new List<ContentBlock>();
}
