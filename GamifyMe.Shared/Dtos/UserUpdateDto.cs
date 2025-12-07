using System;

namespace GamifyMe.Shared.Dtos
{
    public class UserUpdateDto
    {
        public string Type { get; set; } = ""; // "Validation" or "Order"
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public int XpGained { get; set; }
        public int CurrencyGained { get; set; } // Negative for spent? No, usually explicitly gained rewards.
        public DateTime Date { get; set; }
    }
}
