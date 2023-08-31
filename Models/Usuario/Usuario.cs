namespace ManejoPresupuestos.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string EmailNormalizado { get; set; }
        public string PassWordHash { get; set; }
    }
}
