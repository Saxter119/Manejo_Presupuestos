using System.Security.Claims;

namespace ManejoPresupuestos.Servicios
{
    public interface IUssersService
    {
        int ObtenerUsuarioId();
    }

    public class UssersService : IUssersService
    {
        private readonly HttpContext httpContext;
        public UssersService(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }
        public int ObtenerUsuarioId()
        {
            if (httpContext.User.Identity.IsAuthenticated)
            {
                var idClaim = httpContext.User
                        .Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

                var id = int.Parse(idClaim.Value);

                return id;
            }
            else
            {
                throw new ApplicationException("El usuario no está autenticado");
            }
        }
    }
}