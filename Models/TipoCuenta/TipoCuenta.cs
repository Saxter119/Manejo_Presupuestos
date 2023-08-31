using System.ComponentModel.DataAnnotations;
using ManejoPresupuestos.Validaciones;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuestos.Models
{
    public class TipoCuenta
    {
        public int Id { get; set; }

        [Remote(action: "VerifyTypeAccountExist", controller: "TiposCuentas")]
        [ConvertToCapLetter]
        [Required(ErrorMessage = "Ta' vacio el campo '{0}'")]
        [Display(Name = "Nombre de la cuenta")]
        public string Nombre { get; set; }

        public int UsuarioId { get; set; }

        public int Orden { get; set; }
    }
}
