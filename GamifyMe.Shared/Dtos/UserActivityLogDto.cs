namespace GamifyMe.Shared.Dtos
{
    public class UserActivityLogDto
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty; // Ex: "Achat : Café", "Validé : Cours de Zumba"
        public int AmountChange { get; set; } // +50 ou -10
        public string Type { get; set; } = string.Empty; // "XP", "Currency"
        public string Icon { get; set; } = string.Empty; // Pour faire joli dans la liste
    }
}