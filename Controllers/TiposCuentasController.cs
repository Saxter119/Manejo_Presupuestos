using Dapper;
using ManejoPresupuestos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ManejoPresupuestos.Servicios;

namespace ManejoPresupuestos.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IUssersService ussersService;
        private readonly IAccountTypeRepository accountTypeRepository;
        public TiposCuentasController(IConfiguration configuration, IAccountTypeRepository accountTypeRepository, IUssersService ussersService)
        {
            this.accountTypeRepository = accountTypeRepository;
            this.ussersService = ussersService;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var tiposCuentas = await accountTypeRepository.Obtener(usuarioId);

            return View(tiposCuentas);

        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipoCuenta)
        {

            if (!ModelState.IsValid)
            {
                return View(tipoCuenta);
            }

            tipoCuenta.UsuarioId = ussersService.ObtenerUsuarioId(); ;

            var yaExisteTipoCuenta = await accountTypeRepository.Existe(tipoCuenta.Nombre, tipoCuenta.UsuarioId);

            if (yaExisteTipoCuenta)
            {
                ModelState.AddModelError(nameof(tipoCuenta.Nombre), $"Ya tienes una cuenta con el nombre '{tipoCuenta.Nombre}' mi pana");

                return View(tipoCuenta);
            }

            await accountTypeRepository.Crear(tipoCuenta);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var tipoCuenta = await accountTypeRepository.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var tipoCuentaExiste = await accountTypeRepository.ObtenerPorId(tipoCuenta.Id, usuarioId);

            if (tipoCuentaExiste is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await accountTypeRepository.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var tipoCuenta = await accountTypeRepository.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var tipoCuenta = await accountTypeRepository.ObtenerPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
           
            await accountTypeRepository.Borrar(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyTypeAccountExist(string nombre)
        {
            var usuarioId = ussersService.ObtenerUsuarioId(); 

            var yaExisteTipoCuenta = await accountTypeRepository.Existe(nombre, usuarioId);

            if (yaExisteTipoCuenta)
            {
                return Json($"El nombre {nombre} ya existe pana miop"); //En el codigo que se ejecuta el navegador, importa el orden del query?
            }

            return Json(true);


        }

        [HttpPost]
        public async Task<IActionResult>Ordenar([FromBody] int[] Ids)
        {

            var usuarioId = ussersService.ObtenerUsuarioId();
            var tiposCuentas = await accountTypeRepository.Obtener(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(x => x.Id);

            var idsTiposCuentasNoPerteneceAlUsuario = Ids.Except(idsTiposCuentas).ToList();

            if(idsTiposCuentasNoPerteneceAlUsuario.Count > 0) return Forbid();
            
            var tiposCuentasOrdenados = Ids.Select((valor, indice)=> new TipoCuenta(){
                Id = valor, Orden = indice}).AsEnumerable();

            await accountTypeRepository.Ordenar(tiposCuentasOrdenados); 

            return Ok();

        }
    }
}
