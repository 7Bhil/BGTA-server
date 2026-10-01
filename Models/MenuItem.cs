using System.Text.Json.Serialization;

namespace BGTA.Server.Models;

/// <summary>
/// Représente un élément de menu BGTA
/// Version complète BMCI adaptée pour Blazor Server (toutes propriétés conservées)
/// </summary>
public class MenuItem
{
    // ðŸ”‘ PROPRIÉTÉS BMCI 100% CONSERVÉES
    public int Id { get; set; }                 // NumMen
    public string Text { get; set; } = "Sans titre"; // LibMen
    public int Level { get; set; }              // NivMen
    public int? ParentId { get; set; }          // NumNivSup
    public string? Icon { get; set; }           // Image1
    public string? IconSelected { get; set; }   // Image2
    public string? ImageInbox { get; set; }     // ImageInbox
    public string? ImageListview { get; set; }  // ImageListview
    public string? DescriptionMenu { get; set; }// DescriptionMenu
    public int Order { get; set; }              // Ordre
    public string? CodeMenu { get; set; }       // CodeMenu
    public bool AfficheInbox { get; set; }      // AfficheInbox
    public int? IdWFStepInValue { get; set; }   // IdWFStepInValue
    public string? Formulaire { get; set; }     // Formulaire
    public bool MasquerFrmTitre { get; set; }   // MasquerFrmTitre
    public string? DescriptionFormulaire { get; set; } // DescriptionFormulaire
    public string? AffichageMultiple { get; set; } // AffichageMultiple
    public string? NomTable { get; set; }       // NomTable
    public bool Supprimer { get; set; }         // Supprimer
    public int? OrdGen { get; set; }            // OrdGen
    public string? CodeControle { get; set; }   // CodeControle
    public string? OptionEtat { get; set; }     // OptionEtat
    public string? NomEtat { get; set; }        // NomEtat
    public string? NumeroPS { get; set; }       // NumeroPS
    public string? Category { get; set; }       // CategorieMenu
    public string? CategorieMenu { get => Category; set => Category = value; } // Alias
    public string? TagListview { get; set; }    // TagListview
    public bool MenuRaccourci { get; set; }     // MenuRaccourci
    public int? APT_ID_FK { get; set; }         // APT_ID_FK
    public bool BeforeConnection { get; set; }  // BeforeConnection
    public bool AfterConnection { get; set; }   // AfterConnection
    public string? Image1Nom { get; set; }      // Image1Nom
    public string? Image2Nom { get; set; }      // Image2Nom
    
    // ðŸ”‘ PROPRIÉTÉS BGTA AJOUTÉES
    public bool IsVisible { get; set; } = true;
    public string? Url { get; set; }            // URL générée pour Blazor
    
    // ðŸ”‘ PROPRIÉTÉS DE NAVIGATION (pour hiérarchie)
    [JsonIgnore]
    public MenuItem? Parent { get; set; }
    public List<MenuItem> Children { get; set; } = new();
}