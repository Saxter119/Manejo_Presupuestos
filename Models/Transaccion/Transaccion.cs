using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuestos.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; } 

        [Range(1, int.MaxValue, ErrorMessage = "Debes selecionar una cuenta")]
        [Display(Name = "Cuenta")]
        public int CuentaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debes selecionar una categoría")]
        [Display(Name ="Categoría")]
        public int CategoriaId { get; set; }

        [Display(Name = "Fecha de transacción")]
        [DataType(DataType.Date)]
        public DateTime FechaTransaccion { get; set; } = DateTime.Today;

        public decimal Monto { get; set; }

        [MaxLength(1000, ErrorMessage ="No puedes introducir más de {0} caracteres")]
        public string Nota { get; set; }

        [Display(Name = "Operación")]
        public TipoOperacion TipoOperacionId { get; set; } = TipoOperacion.Ingresos;

        public string Cuenta { get; set; }

        public string Categoria { get; set; }
    }
}
