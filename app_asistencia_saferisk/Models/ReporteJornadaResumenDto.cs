namespace app_asistencia_saferisk.Models
{
    public class ReporteJornadaResumenDto
    {
        public int TotalJornadas { get; set; }
        public int TotalUsuarios { get; set; }
        public double? TotalHorasNetas { get; set; }
        public double? PromedioHorasNetas { get; set; }
        public double? PorcentajeCumplimientoPromedio { get; set; }
        public int JornadasPuntuales { get; set; }
        public double? PorcentajePuntualidad { get; set; }
    }

}
