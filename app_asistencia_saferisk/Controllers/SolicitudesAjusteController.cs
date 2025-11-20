using app_asistencia_saferisk.Hubs;
using app_asistencia_saferisk.Models;
using app_asistencia_saferisk.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace app_asistencia_saferisk.Controllers
{
    public class SolicitudesAjusteController : Controller
    {

        private readonly SolicitudAjusteService _service;
        private readonly IHubContext<NotificacionHub> _hubContext;


        public SolicitudesAjusteController(SolicitudAjusteService service, IHubContext<NotificacionHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Handles HTTP GET requests to retrieve and display the list of pending requests.
        /// </summary>
        /// <remarks>The returned view model includes all requests currently marked as pending. This
        /// action is typically used to present outstanding items requiring attention in the application
        /// workflow.</remarks>
        /// <returns>An <see cref="IActionResult"/> that renders the view containing the pending requests.</returns>
        [HttpGet]
        public async Task<IActionResult> Pendientes()
        {
            var pendientes = await _service.ListarPendientesAsync();

            var model = new BandejaSolicitudesViewModel
            {
                Pendientes = pendientes
            };

            return View(model);
        }

        /// <summary>
        /// Marks the specified request as attended and redirects to the list of pending requests.
        /// </summary>
        /// <param name="solicitudId">The unique identifier of the request to be marked as attended.</param>
        /// <returns>A redirect result to the action that displays pending requests.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarAtendido(int solicitudId)
        {
            await _service.MarcarAtendidoAsync(solicitudId);
            return RedirectToAction(nameof(Pendientes));
        }

        /// <summary>
        /// Handles HTTP POST requests to create a new adjustment request based on the provided data.
        /// </summary>
        /// <remarks>The user must have a valid session to create an adjustment request. Both 'TipoAjuste'
        /// and 'Descripcion' are required fields in the request. The response includes a success flag, the ID of the
        /// created request, and a message describing the outcome.</remarks>
        /// <param name="request">The request object containing the details of the adjustment to be created. Must include valid values for
        /// 'TipoAjuste' and 'Descripcion'.</param>
        /// <returns>An IActionResult indicating the result of the operation. Returns 200 OK with the new request ID if
        /// successful; otherwise, returns 400 Bad Request for missing required fields or 401 Unauthorized if the user
        /// session is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearSolicitudAjusteRequest request)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            if (usuarioId == 0)
            {
                return Unauthorized(new { success = false, mensaje = "Sesión no válida." });
            }

            if (string.IsNullOrWhiteSpace(request.TipoAjuste) ||
                string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return BadRequest(new { success = false, mensaje = "Tipo de ajuste y descripción son obligatorios." });
            }

            // Aquí podrías sacar el nombre del usuario desde sesión o BD
            var nombreUsuario = HttpContext.Session.GetString("UsuarioNombre") ?? "Colaborador";

            var solicitudId = await _service.CrearSolicitudAsync(
                request.JornadaId,
                usuarioId,
                request.TipoAjuste,
                request.Descripcion
            );

            // NOTIFICACIÓN EN TIEMPO REAL
            await _hubContext.Clients.Group("usuario_24").SendAsync("NuevoAjuste", new
            {
                SolicitudId = solicitudId,
                TipoAjuste = request.TipoAjuste,
                Descripcion = request.Descripcion,
                UsuarioId = usuarioId,
                NombreUsuario = nombreUsuario,
            });


            return Ok(new
            {
                success = true,
                solicitudId,
                mensaje = "Solicitud registrada correctamente."
            });
        }

        [HttpGet]
        public async Task<IActionResult> ContarPendientes()
        {
            var count = await _service.ContarPendientesAsync();
            return Json(new { count });
        }

    }

}

