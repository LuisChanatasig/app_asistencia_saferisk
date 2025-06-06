using System;
using System.Collections.Generic;

namespace app_asistencia_saferisk.Models;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public int? SupervisorId { get; set; }

    public string Nombres { get; set; } = null!;

    public string Ci { get; set; } = null!;

    public string Pass { get; set; } = null!;

    public int Estado { get; set; }

    public DateTime? CreadoEn { get; set; }

    public DateTime? ActualizadoEn { get; set; }

    public virtual ICollection<Usuario> InverseSupervisor { get; set; } = new List<Usuario>();

    public virtual ICollection<Jornadum> Jornada { get; set; } = new List<Jornadum>();

    public virtual Rol Rol { get; set; } = null!;

    public virtual Usuario? Supervisor { get; set; }
}
