namespace GamifyMe.Shared.Dtos
{
    public class LoginResponse
    {
        // Ce nom ("Token") doit correspondre EXACTEMENT
        // à la propriété JSON que ton API retourne
        public string? Token { get; set; }
    }
}