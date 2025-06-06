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

        public AutenticacionController(AutenticacionService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


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

                // Determina la URL de redirección según el RolId
                string redirectUrl;
                switch (user.RolId)
                {
                    case 3: // Empleado
                    case 4: // Sistemas
                        redirectUrl = Url.Action("Index", "Home");
                        break;
                    case 1: // Gerencia
                    case 2: // Supervisor de área
                    case 5: // Contabilidad
                    case 7: // Administrador
                        redirectUrl = Url.Action("Panel", "Dashboard"); // ¡Ajusta a tu pantalla destino!
                        break;
                    default:
                        redirectUrl = Url.Action("Index", "Home"); // Redirige por defecto
                        break;
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
