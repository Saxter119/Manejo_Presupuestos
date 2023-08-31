using Dapper;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuestos.Controllers
{
    public class CategoriasController: Controller
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IUssersService ussersService;

        public CategoriasController(ICategoryRepository categoryRepository, IUssersService ussersService)
        {
            this.categoryRepository = categoryRepository;
            this.ussersService = ussersService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var categorias = await categoryRepository.Obtener(usuarioId);
            return View(categorias);

        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Categorias categoria)
        {
            if(!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = ussersService.ObtenerUsuarioId();

            categoria.UsuarioId = usuarioId;

            await categoryRepository.Crear(categoria);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            if(!ModelState.IsValid)
            {
                return View(ModelState);
            }
            
            var usuarioId = ussersService.ObtenerUsuarioId();
            var categoria = await categoryRepository.ObtenerPorId(id, usuarioId);

            if(categoria == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categorias categoriaEditar)
        {
            if (!ModelState.IsValid)
            {
                return View(ModelState);
            }
            var usuarioId = ussersService.ObtenerUsuarioId();

            var categoriaExiste = await categoryRepository.ObtenerPorId(categoriaEditar.Id, usuarioId);

            if(categoriaExiste == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoriaEditar.UsuarioId = usuarioId;

            var categoriaActualizar = categoryRepository.Actualizar(categoriaEditar);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            if (!ModelState.IsValid)
            {
                return View(ModelState);
            }
            var usuarioId = ussersService.ObtenerUsuarioId();

            var categoria = await categoryRepository.ObtenerPorId(id, usuarioId);

            if (categoria == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            if (!ModelState.IsValid)
            {
                return View(ModelState);
            }
            var usuarioId = ussersService.ObtenerUsuarioId();

            var categoriaExiste = await categoryRepository.ObtenerPorId(id, usuarioId);

            if (categoriaExiste == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await categoryRepository.Borrar(id);

            return RedirectToAction("Index");
        }

    }
}
