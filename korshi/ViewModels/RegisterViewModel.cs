using System.ComponentModel.DataAnnotations;

namespace korshi.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введите имя")]
        [Display(Name = "Имя")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        [Display(Name = "Фамилия")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите номер телефона")]
        [Display(Name = "Номер телефона")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите адрес")]
        [Display(Name = "Адрес")]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите номер квартиры")]
        [Display(Name = "Номер квартиры")]
        [StringLength(10)]
        public string ApartmentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [Display(Name = "Пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Повторите пароль")]
        [Display(Name = "Повтор пароля")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}