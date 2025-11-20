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


        /// <summary>
        /// Generates an attendance report for one or more users within a specified date range and optional filters.
        /// </summary>
        /// <remarks>If both <paramref name="fechaInicio"/> and <paramref name="fechaFin"/> are omitted or
        /// invalid, the report defaults to the current day. If the start date is after the end date, the range is
        /// automatically corrected. The report includes summary and detailed information for the selected users and
        /// date range.</remarks>
        /// <param name="fechaInicio">The start date of the report range, in a format recognized by DateTime.Parse. If null or invalid, defaults
        /// to today or the value of <paramref name="fechaFin"/> if provided.</param>
        /// <param name="fechaFin">The end date of the report range, in a format recognized by DateTime.Parse. If null or invalid, defaults to
        /// today or the value of <paramref name="fechaInicio"/> if provided.</param>
        /// <param name="usuarioId">The identifier of the user for whom to generate the report. Specify 0 or null to include all users.</param>
        /// <param name="tipoJornada">An optional filter specifying the type of work shift to include in the report. If null, all shift types are
        /// included.</param>
        /// <param name="estadoJornada">An optional filter specifying the status of the work shift to include in the report. If null, all statuses
        /// are included.</param>
        /// <returns>An IActionResult containing the attendance report view and its associated data model.</returns>
        [HttpGet]
        public async Task<IActionResult> ReporteAsistencia(
            string? fechaInicio,
            string? fechaFin,
            int? usuarioId,
            string? tipoJornada,
            string? estadoJornada)
        {
            var hoy = DateTime.Today;
            DateTime inicio;
            DateTime fin;

            var tieneFechaInicio = !string.IsNullOrWhiteSpace(fechaInicio);
            var tieneFechaFin = !string.IsNullOrWhiteSpace(fechaFin);

            // 1) Si no manda ninguna fecha -> hoy - hoy
            if (!tieneFechaInicio && !tieneFechaFin)
            {
                inicio = hoy;
                fin = hoy;
            }
            else
            {
                // 2) Intentar parsear inicio
                if (!tieneFechaInicio || !DateTime.TryParse(fechaInicio, out inicio))
                {
                    // Si no hay inicio válido pero sí hay fin válido, lo igualamos al fin.
                    if (tieneFechaFin && DateTime.TryParse(fechaFin, out var finParseado))
                        inicio = finParseado;
                    else
                        inicio = hoy;
                }

                // 3) Intentar parsear fin
                if (!tieneFechaFin || !DateTime.TryParse(fechaFin, out fin))
                {
                    // Si no hay fin válido pero sí hay inicio válido, lo igualamos al inicio.
                    fin = inicio;
                }
            }

            // 4) Corregir rango invertido
            if (inicio > fin)
            {
                var temp = inicio;
                inicio = fin;
                fin = temp;
            }

            // 5) 0 = “todos los usuarios”
            if (usuarioId == 0)
                usuarioId = null;

            // 6) Usuario/rol actual desde sesión
            int usuarioActual = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int rolActual = HttpContext.Session.GetInt32("RolId") ?? 0;

            var usuarios = await _servicio.ListarUsuariosPorPerfilAsync(rolActual, usuarioActual);

            // 7) Llamada al servicio (usa inicio/fin siempre, el SP ya no se encarga del default)
            var (detalles, resumenTotal, resumenMensual) =
                await _servicio.ObtenerReporteADOAsync(
                    inicio,
                    fin,
                    usuarioId,
                    tipoJornada,
                    estadoJornada);

            // 8) ViewModel (tus fechas son no-nullable, así que siempre tienen valor)
            var model = new ReporteJornadaViewModel
            {
                Usuarios = usuarios,
                Detalles = detalles,
                Resumen = resumenTotal,
                ResumenMensual = resumenMensual,
                FechaInicio = inicio,
                FechaFin = fin,
                UsuarioId = usuarioId
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> FiltrosJornadas()
        {

            return View();
        }
    }
}
