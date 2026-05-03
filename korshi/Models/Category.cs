using Microsoft.Extensions.Hosting;

namespace korshi.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty; // wall, market, aid, poll
        public string Color { get; set; } = "#6B7280";    // hex цвет категории
        public string Icon { get; set; } = "bi-tag";     // bootstrap icon class
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}