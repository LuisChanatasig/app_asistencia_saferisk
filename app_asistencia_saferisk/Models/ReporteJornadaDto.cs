namespace app_asistencia_saferisk.Models
{
    public class ReporteJornadaDto
    {
        public int JornadaId { get; set; }
        public int UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string? TipoJornada { get; set; }
        public DateTime? Fecha { get; set; }
        public string? EstadoJornada { get; set; }

        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }
        public TimeSpan? HoraCambioARemoto { get; set; }
        public TimeSpan? HoraCambioAOficina { get; set; }

        public string? IpEntrada { get; set; }
        public string? IpSalida { get; set; }
        public string? ValidaIpOficina { get; set; }

        public decimal? LatEntrada { get; set; }
        public decimal? LonEntrada { get; set; }
        public string? ValidaGpsOficina { get; set; }
        public string? UbicacionOficina { get; set; }

        public decimal? HorasBrutas { get; set; }
        public decimal? HorasEnRango { get; set; }

        public int? MinutosAlmuerzo { get; set; }
        public int? MinutosBreak { get; set; }
        public int? MinutosPermiso { get; set; }
        public int? MinutosTraslado { get; set; }
        public int? MinutosReunion { get; set; }
        public int? MinutosAtraso { get; set; }
        public int? MinutosSalidaAnticipada { get; set; }

        public decimal? HorasNetas { get; set; }
        public decimal? PorcentajeCumplimiento { get; set; }

        public string? Puntualidad { get; set; }
        public string? Semaforo { get; set; }

        public string? EventosDelDia { get; set; }
        public string? Observaciones { get; set; }
    }

    public class ReporteJornadaResumenDto
    {
        public int TotalJornadas { get; set; }
        public int TotalUsuarios { get; set; }
        public decimal? TotalHorasNetas { get; set; }
        public decimal? PromedioHorasNetas { get; set; }
        public decimal? PorcentajeCumplimientoPromedio { get; set; }
        public int JornadasPuntuales { get; set; }
        public decimal? PorcentajePuntualidad { get; set; }
    }

    public class ReporteJornadaResumenMensualDto
    {
        public string? Mes { get; set; }
        public int JornadasEnMes { get; set; }
        public int UsuariosActivosEnMes { get; set; }
        public decimal? TotalHorasNetasMes { get; set; }
        public decimal? PromedioHorasNetasPorJornadaMes { get; set; }
        public decimal? PorcentajeCumplimientoPromedioMes { get; set; }
        public int JornadasPuntualesEnMes { get; set; }
        public decimal? PorcentajePuntualidadMes { get; set; }
    }

}
