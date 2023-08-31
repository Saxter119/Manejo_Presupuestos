using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "Este campo es requerido.")]
        [EmailAddress(ErrorMessage = "Debes introducir un correo electrónico válido.")]
        [Display(Name = "Email")]
        public string Email{ get; set; }
        
        [Required(ErrorMessage = "Este campo es requerido.")]
        [DataType(DataType.Password)]
        [Display(Name ="Contraseña")]
        public string PassWord { get; set; }
    }
}
