using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Mvc;
using ManejoPresupuestos.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using AutoMapper;

namespace ManejoPresupuestos.Controllers
{
    public class CuentasController : Controller
    {
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly IUssersService ussersService;
        private readonly IAccountRepository accountRepository;
        private readonly IMapper mapper;
        private readonly ITransactionRepository transactionRepository;
        private readonly IReportService reportService;

        public CuentasController(IAccountTypeRepository accountTypeRepository, IUssersService ussersService, IAccountRepository accountRepository,
            IMapper mapper, ITransactionRepository transactionRepository, IReportService reportService)
        {
            this.accountTypeRepository = accountTypeRepository;
            this.ussersService = ussersService;
            this.accountRepository = accountRepository;
            this.mapper = mapper;
            this.transactionRepository = transactionRepository;
            this.reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var cuentasConTipoCuenta = await accountRepository.Buscar(usuarioId);

            var modelo = cuentasConTipoCuenta
            .GroupBy(x => x.TipoCuenta)
            .Select(grupo => new IndexCuentasViewModel
            {
                TiposCuenta = grupo.Key,
                Cuentas = grupo.AsEnumerable()
            }).ToList();

            return View(modelo);
        }

        public async Task<IActionResult> Detalle(int id, int mes, int anualidad)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var cuenta = await accountRepository.ObtenerPorId(id, usuarioId);

            if (cuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            ViewBag.Cuenta = cuenta.Nombre;

            var modelo = reportService.ObtenerReporteTransaccionesDetalladasPorCuenta(usuarioId, cuenta.Id,
                mes, anualidad, ViewBag);

            return View(modelo);
        }



        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var tiposCuentas = await accountTypeRepository.Obtener(usuarioId);

            var modelo = new CuentaCrearViewModel();

            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CuentaCrearViewModel cuenta)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var tiposCuenta = accountTypeRepository.ObtenerPorId(cuenta.Id, usuarioId);

            if (tiposCuenta is null)
            {
                return RedirectToAction("NoENcontrado", "Home");
            }

            if (!ModelState.IsValid)
            {
                cuenta.TiposCuentas = await ObtenerTiposCuentas(usuarioId);
                return View(cuenta);
            }

            await accountRepository.Crear(cuenta);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var cuenta = await accountRepository.ObtenerPorId(id, usuarioId);

            if (cuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<CuentaCrearViewModel>(cuenta);

            modelo.TiposCuentas = await ObtenerTiposCuentas(usuarioId);

            return View(modelo);


        }

        [HttpPost]
        public async Task<IActionResult> Editar(CuentaCrearViewModel cuentaEditar)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var cuenta = await accountRepository.ObtenerPorId(cuentaEditar.Id, usuarioId);

            if (cuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var tipoCuenta = accountTypeRepository.ObtenerPorId(cuentaEditar.Id, usuarioId);

            if (tipoCuenta == null) return RedirectToAction("NoEncontrado", "Home");

            await accountRepository.Actualizar(cuentaEditar);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var cuenta = await accountRepository.ObtenerPorId(id, usuarioId);

            if (cuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(cuenta);

        }

        [HttpPost]
        public async Task<IActionResult> BorrarCuenta(int id)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var cuenta = await accountRepository.ObtenerPorId(id, usuarioId);

            if (cuenta == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await accountRepository.Borrar(id);

            return RedirectToAction("Index", "Cuentas");

        }


        public async Task<IEnumerable<SelectListItem>> ObtenerTiposCuentas(int usuarioId)
        {
            var tiposCuentas = await accountTypeRepository.Obtener(usuarioId);
            return tiposCuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }
    }
}