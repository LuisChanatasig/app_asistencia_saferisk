using Microsoft.AspNetCore.Http; // Necesario para las extensiones de Session
using app_asistencia_saferisk.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;
using app_asistencia_saferisk.Models;

namespace app_asistencia_saferisk.Controllers
{
    public class AutenticacionController : Controller
    {
        private readonly AutenticacionService _usuarioService;
        private readonly JornadaService _jornadaService;

        public AutenticacionController(AutenticacionService usuarioService,JornadaService jornadaService)
        {
            _usuarioService = usuarioService;
            _jornadaService = jornadaService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Validates user credentials and initiates a session if authentication is successful.
        /// </summary>
        /// <remarks>On successful authentication, user session variables are set and the response
        /// includes a redirect URL based on the user's role and attendance status. If authentication fails, the
        /// response contains an error message. This method is intended to be called via HTTP POST.</remarks>
        /// <param name="model">An object containing the user's login credentials, including identification and password information. Must
        /// not be null.</param>
        /// <returns>A JSON result indicating whether authentication was successful. If successful, includes a redirect URL for
        /// the next page; otherwise, includes an error message.</returns>
        [HttpPost]
        public async Task<IActionResult> Validar([FromBody] LoginR model)
        {
            var user = await _usuarioService.ValidarUsuarioAsync(model.Ci, model.Pass);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", user.UsuarioId);
                HttpContext.Session.SetInt32("RolId", user.RolId);
                HttpContext.Session.SetString("Nombres", user.Nombres ?? "");
                if (user.SupervisorId.HasValue)
                    HttpContext.Session.SetInt32("SupervisorId", user.SupervisorId.Value);

                // Consultar el estado de la jornada de hoy
                var jornadaEstado = await _jornadaService.ObtenerEstadoJornadaHoyAsync(user.UsuarioId);

                string redirectUrl;
                if (jornadaEstado != null && jornadaEstado.Estado == "Presente")
                {
                    if (!string.IsNullOrEmpty(jornadaEstado.TipoJornada) &&
                        jornadaEstado.TipoJornada.Equals("horas_extra", StringComparison.OrdinalIgnoreCase))
                    {
                        redirectUrl = Url.Action("Registrar", "HorasExtra");
                    }
                    else
                    {
                        redirectUrl = Url.Action("Index", "Home");
                    }
                }
                else
                {
                    // Aquí puedes poner más lógica por rol si quieres, por defecto redirige al Home normal
                    switch (user.RolId)
                    {
                        case 3: // Empleado
                        case 4: // Sistemas
                        case 2:   // Supervisor de área
                            redirectUrl = Url.Action("Index", "Home");
                            break;
                        case 1: // Gerencia
                     
                        case 5: // Contabilidad
                        case 7: // Administrador
                            redirectUrl = Url.Action("ReporteAsistencia", "ReporteJornada");
                            break;
                        default:
                            redirectUrl = Url.Action("Index", "Home");
                            break;
                    }
                }

                return Json(new { success = true, redirectUrl });
            }
            else
            {
                return Json(new { success = false, mensaje = "Usuario o contraseña incorrectos, o usuario inactivo." });
            }
        }

    }
}
