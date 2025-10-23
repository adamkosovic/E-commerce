namespace backend.Models
{
    public class User 
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;         // original e-post
        public string NormalizedEmail { get; set; } = string.Empty; // lowercase för unikt index/jämförelse
        public string PasswordHash { get; set; } = string.Empty;  // aldrig spara klartext!
        public string Role { get; set; } = "Customer";            // Customer | Admin
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}