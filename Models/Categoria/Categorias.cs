using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class Categorias
    {
        public int Id{ get; set; }
        [Required(ErrorMessage ="Er campo {0} e' requerido primo")]
        [StringLength(maximumLength:50, ErrorMessage ="El nombre no puede exceder los 50 caracteres.")]
        public string Nombre { get; set; }
        [Display(Name = "Tipo de Operación")]
        public TipoOperacion TipoOperacionId { get; set; }
        public int UsuarioId{ get; set; }
    }
}
