namespace GamifyMe.Shared.Dtos
{
    public class DashboardLogDto
    {
        public DateTime Date { get; set; }
        public string? ActorName { get; set; } // Qui a fait l'action ?
        public string? ScannerName { get; set; } // Qui a scanné ? (Staff)
        public string? ScannedUserName { get; set; } // Qui a été scanné ? (Membre)
        public string? ActionType { get; set; } // "Création", "Scan", "Modification"
        public string? Details { get; set; } // "Objectif : Cours de Zumba"
        public string? Icon { get; set; } // Pour l'affichage MudBlazor
        public string? Color { get; set; } = "Default"; // "Success", "Info", "Warning"
    }
}