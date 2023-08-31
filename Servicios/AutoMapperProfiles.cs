using AutoMapper;
using ManejoPresupuestos.Models;

namespace ManejoPresupuestos.Servicios
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Cuentas, CuentaCrearViewModel>();
            CreateMap<Transaccion, TransaccionActualizarViewModel>().ReverseMap();
        }

    }
}
