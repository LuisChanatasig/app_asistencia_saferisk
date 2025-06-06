using System;
using System.Collections.Generic;

namespace app_asistencia_saferisk.Models;

public partial class Jornadum
{
    public int JornadaId { get; set; }

    public int UsuarioId { get; set; }

    public DateOnly Fecha { get; set; }

    public string TipoJornada { get; set; } = null!;

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public string? Observaciones { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime? CreadoEn { get; set; }

    public DateTime? ActualizadoEn { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual Usuario Usuario { get; set; } = null!;
}
