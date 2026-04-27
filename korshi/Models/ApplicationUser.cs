using Microsoft.AspNetCore.Identity;
using System.Numerics;

namespace korshi.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Личные данные
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // Адрес (пользователь может менять сам)
        public string? Address { get; set; }
        public string? ApartmentNumber { get; set; }

        // Привязка к ЖК
        public int? ComplexId { get; set; }
        public Complex? Complex { get; set; }

        // Статус верификации (админ подтверждает)
        public bool IsVerified { get; set; } = false;
        public bool IsBanned { get; set; } = false;

        // Дата регистрации
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Вычисляемые
        public string FullName => $"{FirstName} {LastName}";
        public string Initials => $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}";
    }
}