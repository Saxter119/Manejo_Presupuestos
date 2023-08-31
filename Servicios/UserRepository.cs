using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;
        public UserRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            using var connection = new SqlConnection(connectionString);

            var usuarioId = await connection.QuerySingleAsync<int>(@"
            INSERT INTO USUARIOS
            (Email, EmailNormalizado, PassWordHash) 
            VALUES(@Email, @EmailNormalizado, @PassWordHash);
            SELECT SCOPE_IDENTITY()", usuario);

            await connection.ExecuteAsync(@"ValoresUsuarioNuevo", new { usuarioId },
                commandType: System.Data.CommandType.StoredProcedure);

            return usuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Usuario>(@"
            SELECT * FROM Usuarios WHERE EmailNormalizado = @EmailNormalizado",
            new {emailNormalizado});
        }
    }
}
