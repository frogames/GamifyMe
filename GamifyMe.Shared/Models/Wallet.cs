using System.ComponentModel.DataAnnotations.Schema;

namespace GamifyMe.Shared.Models
{
    public class Wallet : IEstablishmentScoped
    {
        public Guid EstablishmentId { get; set; }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public int Balance { get; set; } = 0;
        public User User { get; set; } = null!;}
    }