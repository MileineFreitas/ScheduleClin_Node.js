using System.ComponentModel.DataAnnotations;

namespace ScheduleClin.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Informe a senha atual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nova senha")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nova senha")]
    [Compare(nameof(NewPassword), ErrorMessage = "As senhas não conferem.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
