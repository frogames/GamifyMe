namespace GamifyMe.Shared.Dtos
{
    public class DashboardLogDto
    {
        public DateTime Date { get; set; }
        public string ActorName { get; set; } = string.Empty; // Qui a fait l'action ?
        public string ActionType { get; set; } = string.Empty; // "Création", "Scan", "Modification"
        public string Details { get; set; } = string.Empty; // "Objectif : Cours de Zumba"
        public string Icon { get; set; } = string.Empty; // Pour l'affichage MudBlazor
        public string Color { get; set; } = "Default"; // "Success", "Info", "Warning"
    }
}