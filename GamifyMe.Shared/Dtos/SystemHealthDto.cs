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

        // Nouveaux indicateurs détaillés
        public double SocialEngagementScore { get; set; } // % participation (Moteur Social)
        public double StatusInequalityScore { get; set; } // Variance XP Top 10% (Moteur Statut)
        
        public double PacingScore { get; set; } // % joueurs dans la bonne courbe
        public int UsersFinishedTooEarlyCount { get; set; }

        public double HoardingRate { get; set; } // % monnaie non dépensée
        public double InflationRate { get; set; } // Ratio Création/Destruction (30j)

        public List<string> FrictionPoints { get; set; } = new(); // Zones de drop-off
    }
}
