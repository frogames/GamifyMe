namespace GamifyMe.Shared.Models
{
    public class Level : IEstablishmentScoped
    {
        public Guid EstablishmentId { get; set; }

        public int Id { get; set; } // Une clé simple (1, 2, 3...)
        public int LevelNumber { get; set; }
        public int XpRequired { get; set; } // XP total nécessaire pour atteindre ce niveau
        public string Title { get; set; } = string.Empty; // Ex: "Novice", "Expert"
        // On pourrait ajouter ici : public Guid? EstablishmentId { get; set; } pour des courbes par établissement
    }
}