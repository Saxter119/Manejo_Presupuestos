namespace ManejoPresupuestos.Models
{
    public class TransaccionActualizarViewModel: TransaccionCrearViewModel
    {
        public int CuentaAnteriorId { get; set; }
        public decimal MontoAnterior { get; set; }
        public string UrlRetorno { get; set; }
    }
}
