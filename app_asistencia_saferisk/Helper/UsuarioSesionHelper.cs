namespace app_asistencia_saferisk.Helper
{
    using Microsoft.AspNetCore.Http;

    public static class UsuarioSesionHelper
    {
        public static int? UsuarioId(HttpContext httpContext)
            => httpContext.Session.GetInt32("UsuarioId");

        public static int? RolId(HttpContext httpContext)
            => httpContext.Session.GetInt32("RolId");

        public static string Nombres(HttpContext httpContext)
            => httpContext.Session.GetString("Nombres");

        public static int? SupervisorId(HttpContext httpContext)
            => httpContext.Session.GetInt32("SupervisorId");
    }

}
