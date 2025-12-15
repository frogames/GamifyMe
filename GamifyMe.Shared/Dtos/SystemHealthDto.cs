namespace GamifyMe.Shared.Dtos
{
    public class SystemHealthDto
    {
        public int TotalStoreValue { get; set; }
        public int TotalWealthCreation { get; set; }
        public int HealthScore { get; set; } // 0-100 (Observation théorique)
        public int RealHealthScore { get; set; } // 0-100 (Observation basée sur les joueurs)
        
        public int TargetWealth { get; set; } // Calculated from objectives
        public int AverageRealProjectedWealth { get; set; } // Calculated from user activity
        
        public string? RichestUserName { get; set; }
        public int RichestUserWealth { get; set; } // Balance + Inventory Value

        public int CycleDurationMonths { get; set; }

        public int UserCount { get; set; }
        public int MaxUsers { get; set; }

        public List<string> Advice { get; set; } = new();
    }
}
