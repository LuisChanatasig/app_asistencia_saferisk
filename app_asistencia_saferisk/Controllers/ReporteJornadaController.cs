using app_asistencia_saferisk.Models;
using app_asistencia_saferisk.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace app_asistencia_saferisk.Controllers
{
    public class ReporteJornadaController : Controller
    {
        private readonly ReporteService _servicio;

        public ReporteJornadaController(ReporteService servicio)
        {
            _servicio = servicio;
        }


        [HttpGet]
        public async Task<IActionResult> ReporteAsistencia(DateTime? fechaInicio, DateTime? fechaFin, int? usuarioId)
        {
            var inicio = fechaInicio ?? DateTime.Today.AddDays(-7);
            var fin = fechaFin ?? DateTime.Today;
            var (detalles, resumen) = await _servicio.ObtenerReporteADOAsync(inicio, fin, usuarioId);

            var model = new ReporteJornadaViewModel
            {
                Detalles = detalles,
                Resumen = resumen
            };

            return View(model);
        }



        //[HttpGet]
        //public async Task<IActionResult> Get(DateTime fechaInicio, DateTime fechaFin, int? usuarioId = null)
        //{
        //    var data = await _servicio.ObtenerReporteAsync(fechaInicio, fechaFin, usuarioId);
        //    return Ok(data);
        //}

    }
}
