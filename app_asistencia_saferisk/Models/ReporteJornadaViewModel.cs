namespace app_asistencia_saferisk.Models
{
    public class ReporteJornadaViewModel
    {
        public IEnumerable<Usuario> Usuarios { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int? UsuarioId { get; set; }
        public List<ReporteJornadaDto> Detalles { get; set; }
        public ReporteJornadaResumenDto Resumen { get; set; }
       
        public List<ReporteJornadaResumenMensualDto> ResumenMensual { get; set; }
    }

}
