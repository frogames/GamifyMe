namespace GamifyMe.Shared.Dtos
{
    // Représente le solde d'UN portefeuille (ex: "DOC_POINTS", 100)
    public class WalletBalanceDto
    {
        public string? CurrencyCode { get; set; } // ex: "DOC_POINTS", "CREDITS"
        public int Balance { get; set; }
    }
}