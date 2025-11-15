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

            // --- ÉTUDES & TRAVAIL ---
            { "Book", Icons.Material.Filled.MenuBook },
            { "School", Icons.Material.Filled.School },
            { "Pen", Icons.Material.Filled.Edit },
            { "Laptop", Icons.Material.Filled.Laptop },
            { "Computer", Icons.Material.Filled.Computer },
            { "Brain", Icons.Material.Filled.Psychology },
            { "Library", Icons.Material.Filled.LocalLibrary },
            { "Briefcase", Icons.Material.Filled.Work },
            { "Lightbulb", Icons.Material.Filled.Lightbulb },

            // --- BOUTIQUE & CONSO ---
            { "ShoppingBag", Icons.Material.Filled.ShoppingBag },
            { "Cart", Icons.Material.Filled.ShoppingCart },
            { "Tag", Icons.Material.Filled.LocalOffer },
            { "Ticket", Icons.Material.Filled.LocalActivity },
            { "QrCode", Icons.Material.Filled.QrCode },
            { "Shirt", Icons.Material.Filled.Checkroom },
            { "Coffee", Icons.Material.Filled.LocalCafe },
            { "Pizza", Icons.Material.Filled.LocalPizza },
            { "Drink", Icons.Material.Filled.LocalDrink },
            { "Restaurant", Icons.Material.Filled.Restaurant },
            { "Euro", Icons.Material.Filled.Euro },
            { "Money", Icons.Material.Filled.AttachMoney },

            // --- LOISIRS & TECH ---
            { "Gamepad", Icons.Material.Filled.VideogameAsset },
            { "Headphones", Icons.Material.Filled.Headphones },
            { "Music", Icons.Material.Filled.MusicNote },
            { "Camera", Icons.Material.Filled.PhotoCamera },
            { "Phone", Icons.Material.Filled.Smartphone },
            { "Palette", Icons.Material.Filled.Palette },
            { "Brush", Icons.Material.Filled.Brush },
            { "Map", Icons.Material.Filled.Map },
            { "Explore", Icons.Material.Filled.Explore },
            { "Home", Icons.Material.Filled.Home },
            { "Lock", Icons.Material.Filled.Lock },
            { "Key", Icons.Material.Filled.VpnKey },
            { "Time", Icons.Material.Filled.AccessTime },
            { "Calendar", Icons.Material.Filled.CalendarToday }
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