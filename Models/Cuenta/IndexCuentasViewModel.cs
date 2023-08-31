namespace ManejoPresupuestos.Models
{
    public class IndexCuentasViewModel
    {
        public string TiposCuenta { get; set; }

        public IEnumerable<Cuentas> Cuentas { get; set; }

        public decimal Balance => Cuentas.Sum(x => x.Balance);
    }
}