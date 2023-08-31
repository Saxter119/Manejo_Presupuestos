using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuestos.Models;
using ManejoPresupuestos.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;


namespace ManejoPresupuestos.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IUssersService ussersService;
        private readonly IAccountRepository accountRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IMapper mapper;
        private readonly IReportService reportService;
        private readonly ITransactionRepository transactionRepository;

        public TransaccionesController(IUssersService ussersService, ITransactionRepository transactionRepository, IAccountRepository accountRepository,
            ICategoryRepository categoryRepository, IMapper mapper, IReportService reportService)
        {
            this.ussersService = ussersService;
            this.accountRepository = accountRepository;
            this.categoryRepository = categoryRepository;
            this.mapper = mapper;
            this.reportService = reportService;
            this.transactionRepository = transactionRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int mes, int anualidad)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var modelo = await reportService.ObtenerReporteTransaccionesDetalladas(usuarioId, mes,
                anualidad, ViewBag);

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Semanal(int mes, int anualidad)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = await reportService.ObtenerReporteSemanal(usuarioId, mes, anualidad, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x =>
             new ResultadoObtenerPorSemana
             {
                 Semana = x.Key,
                 Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingresos).Select(x => x.Monto).FirstOrDefault(),
                 Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gastos).Select(x => x.Monto).FirstOrDefault(),


             }).ToList();

            if (anualidad == 0 || mes == 0)
            {
                DateTime hoy = DateTime.Today;
                anualidad = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(anualidad, mes, 1);

            var diasDelMes = Enumerable.Range(fechaReferencia.Day, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count; i++)
            {
                var semana = i + 1;

                var fechaInicio = new DateTime(anualidad, mes, diasSegmentados[i].First());

                var fechaFin = new DateTime(anualidad, mes, diasSegmentados[i].Last());

                var grupoSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if (grupoSemana == null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();

            var modelo = new ReporteSemanalViewModel();

            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Mensual(int anualidad)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            if (anualidad == 0)
            {
                anualidad = DateTime.Today.Year;
            }

            var transaccionesPorMes = await transactionRepository.ObtenerPorMes(usuarioId, anualidad);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes)
                .Select(x => new ResultadoObtenerPorMes
                {
                    Mes = x.Key,

                    Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingresos)
                    .Select(x => x.Monto).FirstOrDefault(),

                    Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gastos)
                    .Select(x => x.Monto).FirstOrDefault()

                }).ToList();

            for (int mes = 1; mes <= 12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);

                var fechaReferencia = new DateTime(anualidad, mes, 1);

                if (transaccion == null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();

            modelo.anualidad = anualidad;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }

        public IActionResult ExcelReporte()
        {
            return View();

        }

        [HttpGet]
        public async Task<FileResult> ReporteExcelPorMes(int mes, int anualidad)
        {
            var fechaInicio = new DateTime(anualidad, mes, 1);

            var fechaFin = new DateTime(anualidad, mes, 1).AddMonths(1).AddDays(-1);

            var usuarioId = ussersService.ObtenerUsuarioId();

            var transacciones = await transactionRepository.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Reporte de transacciones ~{fechaFin.ToShortDateString()}.xlsx";

            return GeneralExcel(nombreArchivo, transacciones);
        }

        private FileResult GeneralExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");

            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoría"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto")
            });

            foreach (var transaccion in transacciones)
            {
                dataTable.Rows.Add(
                    transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId);
            }


            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);

                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        nombreArchivo);
                }
            }
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAnualidad(int anualidad)
        {
            var fechaInicio = new DateTime(anualidad, 1, 1);

            var fechaFin = new DateTime(anualidad).AddYears(1).AddDays(-1);

            var usuarioId = ussersService.ObtenerUsuarioId();

            var transacciones = await transactionRepository.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                UsuarioId = usuarioId
            });

            var nombreArchivo = $"Manejo de presupuestos {fechaInicio}.xlsx";

            return GeneralExcel(nombreArchivo, transacciones);
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {

            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(100);

            var usuarioId = ussersService.ObtenerUsuarioId();

            var transacciones = await transactionRepository.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                UsuarioId = usuarioId
            });

            var nombreArchivo = $"Manejo de presupuestos {DateTime.Today.ToString("MMMM yyyy")}.xlsx";

            return GeneralExcel(nombreArchivo, transacciones);

        }

        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var transacciones = await transactionRepository.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                FechaInicio = start,
                FechaFin = end,
                UsuarioId = usuarioId
            });

            var eventosCalendario = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                start = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"),
                end = transaccion.FechaTransaccion.ToString("yyyy-MM-dd"), 
                color = transaccion.TipoOperacionId == TipoOperacion.Gastos ? "red" : "green"
            });

            return Json(eventosCalendario);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var transacciones = await transactionRepository.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                FechaInicio = fecha,
                FechaFin = fecha,
                UsuarioId = usuarioId
            });

            return Json(transacciones);
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var modelo = new TransaccionCrearViewModel();

            modelo.Cuentas = await ObtenerCuentas(usuarioId);

            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCrearViewModel modelo)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);

                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

                return View(modelo);
            }

            var cuenta = await accountRepository.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await categoryRepository.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;

            if (modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.Monto *= -1;
            }

            await transactionRepository.Crear(modelo);

            return RedirectToAction("Index");

        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var transaccion = await transactionRepository.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = mapper.Map<TransaccionActualizarViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;

            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);

            modelo.Cuentas = await ObtenerCuentas(usuarioId);

            modelo.UrlRetorno = urlRetorno;

            return View(modelo);

        }

        [HttpPost]
        public async Task<IActionResult> Editar(TransaccionActualizarViewModel modelo)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await accountRepository.ObtenerPorId(modelo.CuentaId, usuarioId);

            var categoria = await categoryRepository.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (cuenta == null || categoria == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<Transaccion>(modelo);

            if (modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                transaccion.Monto *= -1;
            }

            await transactionRepository.Actualizar(transaccion, modelo.CuentaAnteriorId, modelo.MontoAnterior);

            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();

            var transaccion = await transactionRepository.ObtenerPorId(id, usuarioId);

            if (transaccion == null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await transactionRepository.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await accountRepository.Buscar(usuarioId);

            return cuentas.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));

        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await categoryRepository.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = ussersService.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);

            return Ok(categorias);

        }

    }
}

