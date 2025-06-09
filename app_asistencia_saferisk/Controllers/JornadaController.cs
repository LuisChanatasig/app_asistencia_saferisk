using app_asistencia_saferisk.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

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


        // 4. Registro de eventos
        [HttpPost]
        public async Task<IActionResult> RegistrarEvento([FromBody] RegistrarEventoRequest req)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return Json(new { success = false, mensaje = "Sesión expirada." });

            // Obtener el estado y jornada_id
            var estado = await _jornadaService.ObtenerEstadoJornadaHoyAsync(usuarioId.Value);

            int? jornadaId = (int?)(estado?.GetType().GetProperty("jornadaId")?.GetValue(estado) ?? null);

            if (jornadaId == null)
                return Json(new { success = false, mensaje = "No tienes una jornada abierta hoy." });

            var (ok, mensaje) = await _jornadaService.RegistrarEventoAsync(
                jornadaId.Value,
                req.TipoEventoCodigo,
                req.Observaciones,
                "Web",
                req.Ip,
                req.Latitud,
                req.Longitud
            );

            return Json(new { success = ok, mensaje });
        }

        //Cerrar Jornada
        [HttpPost]
        public async Task<IActionResult> CerrarJornada([FromBody] int jornadaId)
        {
            var result = await _jornadaService.CerrarJornadaAsync(jornadaId);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> CerrarJornadaExtra([FromBody] CerrarJornadaRequest req)
        {
            var result = await _jornadaService.CerrarJornadaAsync(req.JornadaId);
            return Json(result);
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
    // Modelo para el request de los eventos 
    public class RegistrarEventoRequest
    {
        public string TipoEventoCodigo { get; set; }
        public string Observaciones { get; set; }
        public string Ip { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }
    public class CerrarJornadaRequest
    {
        public int JornadaId { get; set; }
    }

}
