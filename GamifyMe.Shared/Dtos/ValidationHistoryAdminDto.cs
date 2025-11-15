using System;

namespace GamifyMe.Shared.Dtos
{
    public class ValidationHistoryAdminDto
    {
        public DateTime Date { get; set; }
        public string ObjectiveTitle { get; set; } = string.Empty;
        public string ScannedUserName { get; set; } = string.Empty; // Le nom du joueur
        public int XpGained { get; set; }
        public int DocPointsGained { get; set; }
    }
}