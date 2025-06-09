namespace app_asistencia_saferisk.Models
{
    public class EstadoJornadaDto
    {
        public string Estado { get; set; }
        public bool PuedeRegistrar { get; set; }
        public int? JornadaId { get; set; }
        public string TipoJornada { get; set; } // <-- AGREGADO
    }

}
