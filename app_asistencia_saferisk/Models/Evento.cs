using System;
using System.Collections.Generic;

namespace app_asistencia_saferisk.Models;

public partial class Evento
{
    public int EventoId { get; set; }

    public int JornadaId { get; set; }

    public int TipoEventoId { get; set; }

    public string? Observaciones { get; set; }

    public string? Dispositivo { get; set; }

    public string? IpRegistro { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public DateTime? CreadoEn { get; set; }

    public TimeOnly? HoraRegistro { get; set; }

    public virtual Jornadum Jornada { get; set; } = null!;

    public virtual TipoEvento TipoEvento { get; set; } = null!;
}
