

namespace BGTA.Server.Models.Shared.DTOs;

public class LigneActiviteDto
{
    public int ACT_NUMERO_ORDRE { get; set; }
    public string ACT_CODE { get; set; }= string.Empty;
    public string ACT_DESCRIPTION { get; set; }= string.Empty;
    public int MEX_ID_FK { get; set; }
    public decimal ACT_MONTANT_AE_TOTAL { get; set; }
    public decimal ACT_MONTANT_CP_TOTAL { get; set; }
    public DateTime ACT_DATE_DEBUT { get; set; } = DateTime.Today;
    public DateTime ACT_DATE_FIN { get; set; } = DateTime.Today.AddMonths(3);
    public decimal ACT_POIDS { get; set; }
    public int TRS_ID_RESPONSABLE_FK { get; set; }
    public int TRS_ID_STRUCTURE_RESP_FK { get; set; }
    public int? TRS_ID_STRUCTURE_ASSOCIEE_FK { get; set; }
    public int? TYM_ID_FK { get; set; }
    public int? MDP_ID_FK { get; set; }
    public int? SFI_ID_PRINCIPALE_FK { get; set; }
    public int? TRS_ID_ORGANE_CONTROLE_FK { get; set; }
    public int? AEG_ID_FK { get; set; }
    public string? ACT_CONTRIBUTION_ODD { get; set; }
    public string? ACT_PILIER_PAG { get; set; }
    public string? ACT_SENSIBILITE_GENRE { get; set; }
    public string? ACT_OBSERVATIONS { get; set; }
    public string? ACT_STATUT { get; set; } = "PLANIFIE";
    }