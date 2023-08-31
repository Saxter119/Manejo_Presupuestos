using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Servicios
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly string connectionString;
        private readonly IUssersService ussersService;

        public CategoryRepository(IConfiguration configuration, IUssersService ussersService)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
            this.ussersService = ussersService;
        }
         
        public async Task Crear(Categorias categoria)
        {
            using var connection = new SqlConnection(connectionString);
            //Remember this returns An Generic
            var id = await connection.QuerySingleAsync<int>(@"INSERT into Categorias(Nombre, TipoOperacionId, UsuarioId) Values(@Nombre, @TipoOperacionId, @UsuarioId); 
Select Scope_Identity();", categoria);

            categoria.Id = id;
        }

        public async Task<IEnumerable<Categorias>> Obtener(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Categorias>(@"SELECT * FROM Categorias WHERE usuarioId = @usuarioId", new { usuarioId });
        }
        public async Task<IEnumerable<Categorias>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Categorias>(@"SELECT * FROM Categorias WHERE usuarioId = @usuarioId 
                                                            And TipoOperacionId = @tipoOperacionId", new { usuarioId, tipoOperacionId });
        }

        public async Task<Categorias> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryFirstOrDefaultAsync<Categorias>(@"SELECT * FROM Categorias WHERE Id = @id AND UsuarioId = @usuarioId", new {id, usuarioId});
        }

        public async Task Actualizar(Categorias categoria)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"UPDATE Categorias SET Nombre = @Nombre, TipoOperacion = @TipoOperacion WHERE id = @Id AND usuarioId = @usuarioId", new {categoria});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
             await connection.ExecuteAsync(@"DELETE FROM Categorias WHERE id = @id", new {id});

        }

    }
}
