namespace korshi.Models
{
    public class FinancialReport
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty; // имя файла на диске
        public string OriginalName { get; set; } = string.Empty; // оригинальное имя
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string FileSizeFormatted => FileSize switch
        {
            < 1024 => $"{FileSize} Б",
            < 1024 * 1024 => $"{FileSize / 1024} КБ",
            _ => $"{FileSize / (1024 * 1024)} МБ"
        };
    }
}