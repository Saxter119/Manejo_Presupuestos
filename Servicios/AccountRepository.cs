using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string connectionString;
        public AccountRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuentas cuentas)
        {
            var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Cuentas(Nombre, TipoCuentaId, Balance, Descripcion)
            VALUES(@Nombre, @TipoCuentaId, @Balance, @Descripcion); 
            SELECT SCOPE_IDENTITY();", cuentas);

            cuentas.Id = id; 
        } 

        public async Task<IEnumerable<Cuentas>> Buscar(int usuarioId)
        { 
            var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Cuentas>(@"SELECT Cuentas.Id, Cuentas.Nombre, Balance, TC.Nombre AS TipoCuenta
FROM Cuentas
INNER JOIN TiposCuentas TC
ON TC.Id = Cuentas.TipoCuentaId
WHERE UsuarioId = @UsuarioId
ORDER BY TC.Orden", new {usuarioId});
        }

        public async Task<Cuentas> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Cuentas>(@"SELECT Cuentas.Id, Cuentas.Nombre, Balance, Descripcion, TipoCuentaId
FROM Cuentas
INNER JOIN TiposCuentas TC
ON TC.Id = Cuentas.TipoCuentaId
WHERE Cuentas.Id = @Id AND usuarioId = @usuarioId", new { id, usuarioId });
        }

        public async Task Actualizar(CuentaCrearViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"Update Cuentas
SET Nombre = @Nombre, Balance = @Balance, Descripcion = @Descripcion,
TipoCuentaId = @TipoCuentaId WHERE Id = @Id", cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"DELETE Cuentas where id = @id", new { id });
        }
    }
}