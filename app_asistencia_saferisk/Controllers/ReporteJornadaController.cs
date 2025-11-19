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
        public async Task<IActionResult> ReporteAsistencia(string fechaInicio, string fechaFin, int? usuarioId)
        {
            DateTime inicio;
            DateTime fin;

            // Intentar parsear las fechas, si falla, usar un rango predeterminado (últimos 7 días)
            if (!DateTime.TryParse(fechaInicio, out inicio))
                inicio = DateTime.Today.AddDays(-7);
            if (!DateTime.TryParse(fechaFin, out fin))
                fin = DateTime.Today;

            // Asegurarse de que fechaInicio no sea posterior a fechaFin
            if (inicio > fin)
            {
                // Puedes manejar este error como prefieras, por ejemplo, intercambiando las fechas
                // o mostrando un mensaje de error al usuario.
                // Para este ejemplo, simplemente las intercambiamos.
                DateTime temp = inicio;
                inicio = fin;
                fin = temp;
            }

            // Si usuarioId es 0, se interpreta como "todos los usuarios" (se pasa null al servicio)
            if (usuarioId == 0)
                usuarioId = null;

            // Obtener información del usuario y rol de la sesión
            // Asegúrate de que "UsuarioId" y "RolId" estén siendo establecidos en la sesión en otro lugar.
            int usuarioActual = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int rolActual = HttpContext.Session.GetInt32("RolId") ?? 0;

            // Listar usuarios según el perfil (asumiendo que _servicio.ListarUsuariosPorPerfilAsync existe)
            var usuarios = await _servicio.ListarUsuariosPorPerfilAsync(rolActual, usuarioActual);

            // Llama al servicio para obtener los tres conjuntos de resultados
            // Desestructuramos la tupla en sus tres componentes
            var (detalles, resumenTotal, resumenMensual) = await _servicio.ObtenerReporteADOAsync(inicio, fin, usuarioId);

            // Crea e inicializa el ViewModel con todos los datos
            var model = new ReporteJornadaViewModel
            {
                Usuarios = usuarios,
                Detalles = detalles,
                Resumen = resumenTotal, // Asignamos el resumen total
                ResumenMensual = resumenMensual, // ¡Asignamos el nuevo resumen mensual!
                FechaInicio = inicio,
                FechaFin = fin,
                UsuarioId = usuarioId
            };

            // Retorna la vista con el ViewModel actualizado
            return View(model);
        }

        //[HttpGet]
        //public async Task<IActionResult> _FiltrosJornadas(DateTime fechaInicio, DateTime fechaFin, int? usuarioId = null)
        //{
        //    var data = await _servicio.ObtenerReporteAsync(fechaInicio, fechaFin, usuarioId);
        //    return Ok(data);
        //}


        [HttpGet]
        public async Task<IActionResult> FiltrosJornadas()
        {

            return View();
        }
    }
}
