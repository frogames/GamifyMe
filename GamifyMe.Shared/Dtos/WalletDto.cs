namespace GamifyMe.Shared.Dtos
{
    public class WalletDto
    {
        public string CurrencyCode { get; set; } = string.Empty;
        public int Balance { get; set; }
        public int? Level { get; set; }
        public int? XpForCurrentLevel { get; set; }
        public int? XpForNextLevel { get; set; }
    }
}