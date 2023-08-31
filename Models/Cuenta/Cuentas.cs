using System.ComponentModel.DataAnnotations;
using ManejoPresupuestos.Validaciones;

namespace ManejoPresupuestos.Models
{
    public class Cuentas
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="El campo '{0}' es obligatorio, mi pana")]
        [StringLength(maximumLength:50, MinimumLength = 3)]
        [ConvertToCapLetter]
        public string Nombre { get; set; }

        [Display(Name="Tipo de cuenta")]
        public int TipoCuentaId { get; set; }

        public decimal Balance { get; set; }

        [StringLength(maximumLength:1000)]
        [Display(Name ="Descripción")]
        public string Descripcion { get; set; }
        public string TipoCuenta { get; set; }                        
    }
}