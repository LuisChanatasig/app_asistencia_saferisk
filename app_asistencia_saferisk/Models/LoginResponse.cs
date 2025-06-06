namespace app_asistencia_saferisk.Models
{
    public class LoginResponse
    {
        public int UsuarioId { get; set; }
        public string Nombres { get; set; }
        public int RolId { get; set; }
        public int Estado { get; set; }
        public int? SupervisorId { get; set; }
    }

}
