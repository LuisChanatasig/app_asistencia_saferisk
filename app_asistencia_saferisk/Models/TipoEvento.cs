using System;
using System.Collections.Generic;

namespace app_asistencia_saferisk.Models;

public partial class TipoEvento
{
    public int TipoEventoId { get; set; }

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int Estado { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}
