namespace app_asistencia_saferisk.Models
{
    public class ReporteJornadaDto
    {
        public int JornadaId { get; set; }
        public int UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public string? TipoJornada { get; set; }
        public DateTime?  Fecha { get; set; }
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
        public double? HorasBrutas { get; set; }
        public double? HorasEnRango { get; set; }
        public int? MinutosAlmuerzo { get; set; }
        public int? MinutosBreak { get; set; }
        public int? MinutosPermiso { get; set; }
        public int? MinutosTraslado { get; set; }
        public int? MinutosAtraso { get; set; }
        public int? MinutosSalidaAnticipada { get; set; }
        public double? HorasNetas { get; set; }
        public double? PorcentajeCumplimiento { get; set; }
        public string? Puntualidad { get; set; }
        public string? Semaforo { get; set; }
        public string? EventosDelDia { get; set; }
        public string? Observaciones { get; set; }
    }

}
