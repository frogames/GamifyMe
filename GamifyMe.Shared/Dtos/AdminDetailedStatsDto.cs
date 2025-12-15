
using System;
using System.Collections.Generic;

namespace GamifyMe.Shared.Dtos
{
    public class AdminDetailedStatsDto
    {
        public required StatsKeyFiguresDto KeyFigures { get; set; }
        public required List<StatsUserGrowthDto> UserGrowth { get; set; }
        public required List<StatsObjectiveKpiDto> ObjectiveKpis { get; set; }
        public List<StatsTopItemDto>? TopSoldItems { get; set; }
    }

    public class StatsKeyFiguresDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersLast30Days { get; set; }
        public int ActiveUsersLast30Days { get; set; }
        public long TotalXpDistributed { get; set; }
        public int TotalValidations { get; set; }
    }

    public class StatsUserGrowthDto
    {
        public required string Period { get; set; } // e.g. "MMM yyyy"
        public int NewUsers { get; set; }
        public int CumulativeUsers { get; set; }
    }

    public class StatsObjectiveKpiDto
    {
        public required string ObjectiveTitle { get; set; }
        public int ValidationCount { get; set; }
        public int UniqueUsers { get; set; }
        public double CompletionRate { get; set; } // Percentage of Total Users
    }

    public class StatsTopItemDto
    {
        public required string ItemName { get; set; }
        public int QuantitySold { get; set; }
        public int TotalRevenue { get; set; }
    }
}
