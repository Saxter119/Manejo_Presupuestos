using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly string connectionString;

        public TransactionRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Transaccion trasaccion)
        {
            var connection = new SqlConnection(connectionString);

            var id = await connection.QuerySingleAsync<int>(@"Transacciones_Insertar", new
            {
                trasaccion.CategoriaId,
                trasaccion.CuentaId,
                trasaccion.Monto,
                trasaccion.Nota,
                trasaccion.UsuarioId,
                trasaccion.FechaTransaccion
            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ParametroObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id, Monto, t.FechaTransaccion, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId 
FROM Transacciones t 
INNER JOIN Categorias c ON c.Id = t.CategoriaId 
INNER JOIN Cuentas cu ON cu.Id = t.CuentaId 
WHERE t.CuentaId = @CuentaId AND t.UsuarioId = @UsuarioId 
AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Transaccion>(@"SELECT t.Id, Monto, t.FechaTransaccion, nota, c.Nombre as Categoria, cu.Nombre as Cuenta, c.TipoOperacionId 
FROM Transacciones t 
INNER JOIN Categorias c ON c.Id = t.CategoriaId 
INNER JOIN Cuentas cu ON cu.Id = t.CuentaId 
WHERE t.UsuarioId = @UsuarioId 
AND t.FechaTransaccion BETWEEN @FechaInicio AND @FechaFin 
ORDER BY FechaTransaccion DESC", modelo);
        }

        public async Task Actualizar(Transaccion transaccion, int cuentaAnteriorId, decimal montoAnterior)

        {
            var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("Transacciones_Actualizar", new
            {
                transaccion.Id,
                transaccion.FechaTransaccion,
                transaccion.Monto,
                transaccion.CuentaId,
                transaccion.CategoriaId,
                transaccion.Nota,
                cuentaAnteriorId,
                montoAnterior
            }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(@"SELECT Transacciones.*, cat.TipoOperacionId FROM Transacciones
                                                                            INNER JOIN  Categorias cat ON cat.Id = Transacciones.CategoriaId
                                                                            WHERE Transacciones.Id = @Id AND Transacciones.usuarioId = @usuarioId", new { id, usuarioId });
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"SELECT 
DATEDIFF(d, @fechaInicio, FechaTransaccion) / 7 + 1 as Semana, 
SUM(Monto) as Monto, Categorias.TipoOperacionId
FROM Transacciones
INNER JOIN Categorias ON Categorias.Id  = Transacciones.CategoriaId
WHERE Transacciones.UsuarioId = @usuarioId AND FechaTransaccion BETWEEN @fechaInicio AND @fechaFin 
GROUP BY DATEDIFF(D, @fechaInicio, FechaTransaccion) / 7, Categorias.TipoOperacionId", modelo);
        }

        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int anualidad)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"SELECT MONTH(FechaTransaccion) AS mes,
SUM(Monto) AS Monto, Categorias.TipoOperacionId FROM Transacciones 
INNER JOIN Categorias ON Categorias.Id = Transacciones.CategoriaId
WHERE Transacciones.UsuarioId = @usuarioId 
AND YEAR(FechaTransaccion) = @anualidad
GROUP BY MONTH(FechaTransaccion), Categorias.TipoOperacionId", new {usuarioId, anualidad});
        }
        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync("Transacciones_Borrar", new { id }, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
