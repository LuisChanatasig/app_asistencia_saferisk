using app_asistencia_saferisk.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace app_asistencia_saferisk.Controllers
{
    public class JornadaController : Controller
    {
        private readonly JornadaService _jornadaService;

        public JornadaController(JornadaService jornadaService)
        {
            _jornadaService = jornadaService;
        }

        // 1. Obtener el estado actual de la jornada (AJAX GET)
        [HttpGet]
        public async Task<IActionResult> EstadoActual()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return Json(new { estado = "Sesión expirada", puedeRegistrar = false });

            var result = await _jornadaService.ObtenerEstadoJornadaHoyAsync(usuarioId.Value);
            return Json(result);
        }

        // 2. Registrar llegada (AJAX POST)
        [HttpPost]
        public async Task<IActionResult> RegistrarLlegada([FromBody] RegistrarLlegadaRequest req)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return Json(new { success = false, mensaje = "Sesión expirada." });

            var result = await _jornadaService.RegistrarLlegadaAsync(
                usuarioId.Value,
                req.TipoJornada,
                req.Ip,
                req.Latitud,
                req.Longitud
            );
            return Json(result);
        }
        // 3. Timeline del día (AJAX GET)
        [HttpGet]
        public async Task<IActionResult> TimelineHoy()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return Json(new List<object>());

            var timeline = await _jornadaService.ObtenerTimelineHoyAsync(usuarioId.Value);
            return Json(timeline);
        }

        // Si necesitas la vista para el detalle de la jornada, puedes agregarla aquí:
        [HttpGet]
        public IActionResult DetalleHoy()
        {
            // Lógica para cargar el detalle de la jornada (puedes usar servicios, models, etc)
            return View();
        }


    }
    // Modelo para el request de llegada
    public class RegistrarLlegadaRequest
    {
        public string TipoJornada { get; set; }
        public string Ip { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }

}
