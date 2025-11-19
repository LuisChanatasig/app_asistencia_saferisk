namespace app_asistencia_saferisk.Models
{
    public class ReporteJornadaResumenMensualDto
    {
        public string Mes { get; set; } // Formato "yyyy-MM"
        public int JornadasEnMes { get; set; }
        public int UsuariosActivosEnMes { get; set; }
        public double? TotalHorasNetasMes { get; set; }
        public double? PromedioHorasNetasPorJornadaMes { get; set; }
        public double? PorcentajeCumplimientoPromedioMes { get; set; }
        public int JornadasPuntualesEnMes { get; set; }
        public double? PorcentajePuntualidadMes { get; set; }
    }
}
