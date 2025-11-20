namespace app_asistencia_saferisk.Models
{
    public class SolicitudAjusteDto
    {
        public int SolicitudId { get; set; }
        public int? JornadaId { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string TipoAjuste { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public DateTime CreadoEl { get; set; }
        public DateTime? FechaJornada { get; set; }
        public string TipoJornada { get; set; }
        public string EstadoJornada { get; set; }
    }

    public class BandejaSolicitudesViewModel
    {
        public List<SolicitudAjusteDto> Pendientes { get; set; } = new();
    }


    public class CrearSolicitudAjusteRequest
    {
        public int? JornadaId { get; set; }
        public string TipoAjuste { get; set; }
        public string Descripcion { get; set; }
    }

}
