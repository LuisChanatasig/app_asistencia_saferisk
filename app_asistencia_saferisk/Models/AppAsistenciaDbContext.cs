using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace app_asistencia_saferisk.Models;

public partial class AppAsistenciaDbContext : DbContext
{
    public AppAsistenciaDbContext()
    {
    }

    public AppAsistenciaDbContext(DbContextOptions<AppAsistenciaDbContext> options)
        : base(options)
    {
    }
    public DbSet<ReporteJornadaDto> reporteJornadaDtos { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<Jornadum> Jornada { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<TipoEvento> TipoEventos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=app_asistencia_db;User Id=sa;Password=Sur2o22--;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReporteJornadaDto>().HasNoKey();

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.EventoId).HasName("PK__evento__1850C3AD5558AE45");

            entity.ToTable("evento");

            entity.HasIndex(e => e.JornadaId, "idx_evento_jornada_id");

            entity.Property(e => e.EventoId).HasColumnName("evento_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creado_en");
            entity.Property(e => e.Dispositivo)
                .HasMaxLength(100)
                .HasColumnName("dispositivo");
            entity.Property(e => e.HoraRegistro).HasColumnName("hora_registro");
            entity.Property(e => e.IpRegistro)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("ip_registro");
            entity.Property(e => e.JornadaId).HasColumnName("jornada_id");
            entity.Property(e => e.Latitud)
                .HasColumnType("decimal(10, 6)")
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasColumnType("decimal(10, 6)")
                .HasColumnName("longitud");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(255)
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoEventoId).HasColumnName("tipo_evento_id");

            entity.HasOne(d => d.Jornada).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.JornadaId)
                .HasConstraintName("FK_evento_jornada_id");

            entity.HasOne(d => d.TipoEvento).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.TipoEventoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_evento_tipo_evento_id");
        });

        modelBuilder.Entity<Jornadum>(entity =>
        {
            entity.HasKey(e => e.JornadaId).HasName("PK__jornada__BDB0F2C74FBA3940");

            entity.ToTable("jornada");

            entity.HasIndex(e => new { e.UsuarioId, e.Fecha }, "idx_jornada_usuario_fecha").IsUnique();

            entity.Property(e => e.JornadaId).HasColumnName("jornada_id");
            entity.Property(e => e.ActualizadoEn)
                .HasColumnType("datetime")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creado_en");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("abierta")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(255)
                .HasColumnName("observaciones");
            entity.Property(e => e.TipoJornada)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_jornada");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Jornada)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_jornada_usuario_id");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__rol__CF32E44385D7106E");

            entity.ToTable("rol");

            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.Estado)
                .HasDefaultValue(1)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TipoEvento>(entity =>
        {
            entity.HasKey(e => e.TipoEventoId).HasName("PK__tipo_eve__30CB549E1734C689");

            entity.ToTable("tipo_evento");

            entity.HasIndex(e => e.Codigo, "UQ__tipo_eve__40F9A206CE25EA4A").IsUnique();

            entity.Property(e => e.TipoEventoId).HasColumnName("tipo_evento_id");
            entity.Property(e => e.Codigo)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValue(1)
                .HasColumnName("estado");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__usuario__2ED7D2AF3CAA24B8");

            entity.ToTable("usuario");

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.ActualizadoEn)
                .HasColumnType("datetime")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.Ci)
                .HasMaxLength(20)
                .HasColumnName("ci");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creado_en");
            entity.Property(e => e.Estado)
                .HasDefaultValue(1)
                .HasColumnName("estado");
            entity.Property(e => e.Nombres)
                .HasMaxLength(250)
                .HasColumnName("nombres");
            entity.Property(e => e.Pass).HasColumnName("pass");
            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.SupervisorId).HasColumnName("supervisor_id");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_usuario_rol_id");

            entity.HasOne(d => d.Supervisor).WithMany(p => p.InverseSupervisor)
                .HasForeignKey(d => d.SupervisorId)
                .HasConstraintName("FK_usuario_supervisor");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
