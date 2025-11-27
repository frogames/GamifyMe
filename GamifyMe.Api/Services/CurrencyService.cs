using System;

namespace GamifyMe.Api.Services
{
    // Simple in‑memory service to hold the name of the primary currency (e.g. "DOC").
    // In a real app this would be persisted (e.g. in a Settings table), but for now an
    // in‑memory singleton is sufficient for the editor to change it at runtime.
    public class CurrencyService
    {
        private string _currencyName = "Crédits"; // default value used throughout the app

        public string CurrencyName
        {
            get => _currencyName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Currency name cannot be empty.");
                _currencyName = value.Trim();
            }
        }
    }
}
