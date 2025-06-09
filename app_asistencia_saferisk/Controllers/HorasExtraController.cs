using app_asistencia_saferisk.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace app_asistencia_saferisk.Controllers
{
    public class HorasExtraController : Controller
    {
        private readonly JornadaService _jornadaService;

        public HorasExtraController(JornadaService jornadaService)
        {
            _jornadaService = jornadaService;
        }

        // 1. Vista para registrar horas extra
        public async Task<IActionResult> Registrar()
        {
            
            return View();
        }
        // 1.1 Estado horas extra
        [HttpGet]
        public async Task<IActionResult> EstadoHorasExtraHoy()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return Json(new { estado = "noIniciado", mensaje = "Sesión expirada" });
            var result = await _jornadaService.ObtenerEstadoHorasExtraHoyAsync(usuarioId.Value);
            return Json(result);
        }



        // 2 Registrar llegada horas extra

        [HttpPost]
        public async Task<IActionResult> RegistrarLlegadaHorasExtra([FromBody] RegistrarLlegadaRequest req)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return Json(new { success = false, mensaje = "Sesión expirada." });

            var result = await _jornadaService.RegistrarLlegadaHorasExtraAsync(
                usuarioId.Value,
                req.TipoJornada,
                req.Ip,
                req.Latitud,
                req.Longitud
            );
            return Json(result);
        }

        // 3. Registrar eventos extra
        [HttpPost]
        public async Task<IActionResult> RegistrarEventoHorasExtra([FromBody] RegistrarEventoHorasExtraRequest req)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null)
                return Json(new { success = false, mensaje = "Sesión expirada." });

            var result = await _jornadaService.RegistrarEventoHorasExtraAsync(
                usuarioId.Value,
                req.TipoEvento,
                req.Observaciones,
                req.Ip,
                req.Latitud,
                req.Longitud
            );
            return Json(result);
        }


        public class RegistrarEventoHorasExtraRequest
        {
            public string TipoEvento { get; set; }
            public string Observaciones { get; set; }
            public string Ip { get; set; }
            public decimal? Latitud { get; set; }
            public decimal? Longitud { get; set; }
        }

    }
}
