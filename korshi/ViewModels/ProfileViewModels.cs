using System.ComponentModel.DataAnnotations;
using korshi.Models;

namespace korshi.ViewModels
{
    // ── Просмотр профиля ──────────────────────────────────
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Post> MyPosts { get; set; } = new();
        public List<Post> LikedPosts { get; set; } = new();
        public int FriendsCount { get; set; }
        public string ActiveTab { get; set; } = "posts";
    }

    // ── Редактирование профиля ────────────────────────────
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        [StringLength(50)]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [StringLength(50)]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Неверный формат телефона")]
        [Display(Name = "Номер телефона")]
        public string? PhoneNumber { get; set; }

        [StringLength(200)]
        [Display(Name = "Адрес")]
        public string? Address { get; set; }

        [StringLength(10)]
        [Display(Name = "Номер квартиры")]
        public string? ApartmentNumber { get; set; }
    }

    // ── Смена пароля ──────────────────────────────────────
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введите текущий пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите новый пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Минимум 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторите новый пароль")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        [Display(Name = "Повтор пароля")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}