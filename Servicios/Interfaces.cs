using ManejoPresupuestos.Models;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;

public interface IAccountTypeRepository
{
    Task Crear(TipoCuenta tipoCuenta);

    Task<bool> Existe(string nombre, int usuarioId);

    Task<IEnumerable<TipoCuenta>> Obtener(int usuarioId);

    Task Actualizar(TipoCuenta tipoCuenta);

    Task<TipoCuenta> ObtenerPorId(int id, int usuarioId);

    Task Borrar(int id);

    Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
}

public interface IAccountRepository
{
    Task Crear(Cuentas cuentas);
    Task<IEnumerable<Cuentas>> Buscar(int usuarioId);
    Task<Cuentas> ObtenerPorId(int id, int usuarioId);
    Task Actualizar(CuentaCrearViewModel cuenta);
    Task Borrar(int id);
}

public interface ICategoryRepository
{
    Task Actualizar(Categorias categoria);
    Task Borrar(int id);
    Task Crear(Categorias categorias);
    Task<IEnumerable<Categorias>> Obtener(int usuarioId);
    Task<IEnumerable<Categorias>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
    Task<Categorias> ObtenerPorId(int id, int usuarioId);
}

public interface ITransactionRepository
{
    Task Actualizar(Transaccion transaccion, int cuentaAnteriorId, decimal montoAnterior);
    Task Borrar(int id);
    Task Crear(Transaccion trasaccion);
    Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ParametroObtenerTransaccionesPorCuenta modelo);
    Task<Transaccion> ObtenerPorId(int id, int usuarioId);
    Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int anualidad);
    Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
    Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
}

public interface IReportService
{
    Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerReporteSemanal(int usuarioId, int mes, int anualidad, dynamic ViewBag);
    Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladas(int usuarioId, int mes, int anualidad, dynamic ViewBag);
    Task<ReporteTransaccionesDetalladas> ObtenerReporteTransaccionesDetalladasPorCuenta(int usuarioId, int cuentaId, int mes, int anualidad, dynamic ViewBag);
}

public interface IUserRepository
{
    Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
    Task<int> CrearUsuario(Usuario usuario);
}
