// N'oublie pas d'ajouter ce using en haut
using System.Collections.Generic;

namespace GamifyMe.Shared.Dtos
{
    public class InfoBarDto
    {
        public string? EstablishmentName { get; set; }

        // --- Section XP (Niveau) ---
        public int Level { get; set; }
        public int CurrentXp { get; set; } // Le solde XP actuel
        public int XpToNextLevel { get; set; } // Pour la barre de progression

        // --- Section "Autres Monnaies" (Générique) ---
        // C'est une liste, pour être flexible et déclinable
        public List<WalletBalanceDto> OtherWallets { get; set; } = new List<WalletBalanceDto>();
    }
}