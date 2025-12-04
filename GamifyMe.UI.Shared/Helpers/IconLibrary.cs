using MudBlazor;

namespace GamifyMe.UI.Shared.Helpers
{
    public static class IconLibrary
    {
        // Dictionnaire : Nom en BDD => Chemin SVG MudBlazor
        public static readonly Dictionary<string, string> IconsMap = new()
        {
            // --- RÉCOMPENSES & GAMIFICATION ---
            { "Star", Icons.Material.Filled.Star },
            { "Trophy", Icons.Material.Filled.EmojiEvents },
            { "Medal", Icons.Material.Filled.MilitaryTech },
            { "Crown", Icons.Material.Filled.CrueltyFree },
            { "Diamond", Icons.Material.Filled.Diamond },
            { "Gift", Icons.Material.Filled.CardGiftcard },
            { "Rocket", Icons.Material.Filled.RocketLaunch },
            { "Fire", Icons.Material.Filled.LocalFireDepartment },
            { "Flash", Icons.Material.Filled.FlashOn },
            { "ThumbUp", Icons.Material.Filled.ThumbUp },
            { "AutoAwesome", Icons.Material.Filled.AutoAwesome },
            { "Bolt", Icons.Material.Filled.Bolt },

            // --- SPORT & SANTÉ ---
            { "Dumbbell", Icons.Material.Filled.FitnessCenter },
            { "Running", Icons.Material.Filled.DirectionsRun },
            { "Walking", Icons.Material.Filled.DirectionsWalk },
            { "Hiking", Icons.Material.Filled.Hiking },
            { "Bike", Icons.Material.Filled.DirectionsBike },
            { "Pool", Icons.Material.Filled.Pool },
            { "Heart", Icons.Material.Filled.Favorite },
            { "Water", Icons.Material.Filled.WaterDrop },
            { "Timer", Icons.Material.Filled.Timer },
            { "Scale", Icons.Material.Filled.MonitorWeight },
            { "Yoga", Icons.Material.Filled.SelfImprovement },
            { "Soccer", Icons.Material.Filled.SportsSoccer },
            { "Basketball", Icons.Material.Filled.SportsBasketball },
            { "Tennis", Icons.Material.Filled.SportsTennis },
            { "Sports", Icons.Material.Filled.Sports },
            { "SportsScore", Icons.Material.Filled.SportsScore },

            // --- ÉTUDES, TRAVAIL & BUREAU ---
            { "Book", Icons.Material.Filled.MenuBook },
            { "School", Icons.Material.Filled.School },
            { "Pen", Icons.Material.Filled.Edit },
            { "Pencil", Icons.Material.Filled.Edit }, // Alias
            { "Laptop", Icons.Material.Filled.Laptop },
            { "Computer", Icons.Material.Filled.Computer },
            { "Brain", Icons.Material.Filled.Psychology },
            { "Library", Icons.Material.Filled.LocalLibrary },
            { "Briefcase", Icons.Material.Filled.Work },
            { "Lightbulb", Icons.Material.Filled.Lightbulb },
            { "Notebook", Icons.Material.Filled.Book }, // Alias
            { "Scissors", Icons.Material.Filled.ContentCut },
            { "Ruler", Icons.Material.Filled.Straighten },
            { "Calculator", Icons.Material.Filled.Calculate },

            // --- BOUTIQUE & CONSO ---
            { "ShoppingBag", Icons.Material.Filled.ShoppingBag },
            { "Cart", Icons.Material.Filled.ShoppingCart },
            { "Tag", Icons.Material.Filled.LocalOffer },
            { "Ticket", Icons.Material.Filled.LocalActivity },
            { "Phone", Icons.Material.Filled.Smartphone },
            { "Map", Icons.Material.Filled.Map },
            { "Explore", Icons.Material.Filled.Explore },
            { "Home", Icons.Material.Filled.Home },
            { "Lock", Icons.Material.Filled.Lock },
            { "Key", Icons.Material.Filled.VpnKey },
            { "Backpack", Icons.Material.Filled.Backpack },
            { "Umbrella", Icons.Material.Filled.Umbrella },

            // --- NOURRITURE & BOISSONS ---
            { "Mug", Icons.Material.Filled.EmojiFoodBeverage },
            { "Cup", Icons.Material.Filled.LocalCafe },
            { "Glass", Icons.Material.Filled.LocalBar },
            { "Bottle", Icons.Material.Filled.LocalDrink },
            { "Camera", Icons.Material.Filled.PhotoCamera },
            { "Mouse", Icons.Material.Filled.Mouse },
            { "Keyboard", Icons.Material.Filled.Keyboard },
            { "Gamepad", Icons.Material.Filled.VideogameAsset },
            { "Battery", Icons.Material.Filled.BatteryFull },
            { "Ball", Icons.Material.Filled.SportsSoccer },
            { "Racket", Icons.Material.Filled.SportsTennis },
            { "Skateboard", Icons.Material.Filled.Skateboarding },
            { "Guitar", Icons.Material.Filled.MusicNote },
            { "Music", Icons.Material.Filled.MusicNote },
            { "Piano", Icons.Material.Filled.Piano },
            { "Palette", Icons.Material.Filled.Palette },
            { "Brush", Icons.Material.Filled.Brush },
            { "Tshirt", Icons.Material.Filled.Accessibility },
            { "Cake", Icons.Material.Filled.Cake },
            { "LunchDining", Icons.Material.Filled.LunchDining },

            // --- DIVERS & FINANCE ---
            { "Euro", Icons.Material.Filled.Euro },
            { "Dollar", Icons.Material.Filled.AttachMoney },
            { "Money", Icons.Material.Filled.AttachMoney },
            { "CreditCard", Icons.Material.Filled.CreditCard },
            { "Wallet", Icons.Material.Filled.AccountBalanceWallet },
            { "Unlock", Icons.Material.Filled.LockOpen },
            { "StarOutline", Icons.Material.Filled.StarOutline },
            { "HeartOutline", Icons.Material.Filled.FavoriteBorder },
            { "DarkMode", Icons.Material.Filled.DarkMode },
            { "Google", Icons.Custom.Brands.Google },
            { "QrCode", Icons.Material.Filled.QrCode },
            { "QrCodeScanner", Icons.Material.Filled.QrCodeScanner }
        };

        // Méthode utilitaire pour récupérer l'icône (avec fallback si introuvable)
        public static string GetIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return Icons.Material.Filled.HelpOutline;

            // Si c'est une ancienne icône FontAwesome (commence par "fas "), on renvoie une icône par défaut ou on gère le cas
            if (iconName.StartsWith("fas ") || iconName.StartsWith("fa ")) return Icons.Material.Filled.Star;

            return IconsMap.ContainsKey(iconName) ? IconsMap[iconName] : Icons.Material.Filled.HelpOutline;
        }
    }
}