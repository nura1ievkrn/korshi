namespace korshi.Models
{
    public class Complex
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public ICollection<ApplicationUser> Residents { get; set; } = new List<ApplicationUser>();
    }
}