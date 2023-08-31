using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuestos.Models
{
    public class CuentaCrearViewModel : Cuentas
    {
        public IEnumerable<SelectListItem> TiposCuentas { get; set; }
    }
}