namespace GamifyMe.Shared.Dtos
{
    public class CreateValidationDto
    {
        // --- NOUVEAUX CHAMPS (Pour le DashboardController) ---

        // "Objective" ou "Profile"
        public string Type { get; set; } = "Objective";

        // L'ID de l'objectif (ou "SCAN_PROFIL" si c'est un scan profil)
        public string QrCode { get; set; } = string.Empty;

        // Le QR Code du joueur qui a été scanné
        public string UserQrCode { get; set; } = string.Empty;


        // --- ANCIENS CHAMPS (Pour compatibilité temporaire) ---
        // On peut les garder pour éviter des erreurs de build si d'autres fichiers les utilisent encore,
        // mais l'objectif est de passer aux nouveaux.
        public string ScannedQrCodeContent { get; set; } = string.Empty;
        public Guid ObjectiveId { get; set; }
    }
}