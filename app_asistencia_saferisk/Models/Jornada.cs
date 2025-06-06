namespace app_asistencia_saferisk.Models
{
    public class Jornada
    {
        public int JornadaId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoJornada { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFin { get; set; }
        public string Estado { get; set; }
        public DateTime CreadoEn { get; set; }
        // ...
    }
}
