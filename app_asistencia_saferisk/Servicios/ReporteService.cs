using app_asistencia_saferisk.Models;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace app_asistencia_saferisk.Servicios
{
    public class ReporteService
    {
        private readonly AppAsistenciaDbContext _dbContext;
        public ReporteService(AppAsistenciaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(List<ReporteJornadaDto> Detalles, ReporteJornadaResumenDto Resumen)> ObtenerReporteADOAsync(DateTime fechaInicio, DateTime fechaFin, int? usuarioId = null)
        {
            var lista = new List<ReporteJornadaDto>();
            ReporteJornadaResumenDto resumen = null;
            string connectionString = _dbContext.Database.GetConnectionString();

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_ReporteJornadaNormal", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fecha_inicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fecha_fin", fechaFin);
                cmd.Parameters.AddWithValue("@usuario_id", (object)usuarioId ?? DBNull.Value);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // --- Primer result set: detalles ---
                    while (await reader.ReadAsync())
                    {
                        var dto = new ReporteJornadaDto
                        {
                            JornadaId = reader.GetInt32(reader.GetOrdinal("JornadaId")),
                            UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                            NombreUsuario = reader["NombreUsuario"] as string,
                            TipoJornada = reader["TipoJornada"] as string,
                            Fecha = reader["Fecha"] == DBNull.Value ? null : (DateTime?)reader["Fecha"],
                            EstadoJornada = reader["EstadoJornada"] as string,
                            HoraEntrada = reader["HoraEntrada"] == DBNull.Value ? null : (TimeSpan?)reader["HoraEntrada"],
                            HoraSalida = reader["HoraSalida"] == DBNull.Value ? null : (TimeSpan?)reader["HoraSalida"],
                            HoraCambioARemoto = reader["HoraCambioARemoto"] == DBNull.Value ? null : (TimeSpan?)reader["HoraCambioARemoto"],
                            HoraCambioAOficina = reader["HoraCambioAOficina"] == DBNull.Value ? null : (TimeSpan?)reader["HoraCambioAOficina"],
                            IpEntrada = reader["IpEntrada"] as string,
                            IpSalida = reader["IpSalida"] as string,
                            ValidaIpOficina = reader["ValidaIpOficina"] as string,
                            LatEntrada = reader["LatEntrada"] == DBNull.Value ? null : (decimal?)reader["LatEntrada"],
                            LonEntrada = reader["LonEntrada"] == DBNull.Value ? null : (decimal?)reader["LonEntrada"],
                            ValidaGpsOficina = reader["ValidaGpsOficina"] as string,
                            UbicacionOficina = reader["UbicacionOficina"] as string,
                            HorasBrutas = reader["HorasBrutas"] == DBNull.Value ? null : (double?)reader["HorasBrutas"],
                            HorasEnRango = reader["HorasEnRango"] == DBNull.Value ? null : (double?)reader["HorasEnRango"],
                            MinutosAlmuerzo = reader["MinutosAlmuerzo"] == DBNull.Value ? null : (int?)reader["MinutosAlmuerzo"],
                            MinutosBreak = reader["MinutosBreak"] == DBNull.Value ? null : (int?)reader["MinutosBreak"],
                            MinutosPermiso = reader["MinutosPermiso"] == DBNull.Value ? null : (int?)reader["MinutosPermiso"],
                            MinutosTraslado = reader["MinutosTraslado"] == DBNull.Value ? null : (int?)reader["MinutosTraslado"],
                            MinutosAtraso = reader["MinutosAtraso"] == DBNull.Value ? null : (int?)reader["MinutosAtraso"],
                            MinutosSalidaAnticipada = reader["MinutosSalidaAnticipada"] == DBNull.Value ? null : (int?)reader["MinutosSalidaAnticipada"],
                            HorasNetas = reader["HorasNetas"] == DBNull.Value ? null : (double?)reader["HorasNetas"],
                            PorcentajeCumplimiento = reader["PorcentajeCumplimiento"] == DBNull.Value ? null : (double?)reader["PorcentajeCumplimiento"],
                            Puntualidad = reader["Puntualidad"] as string,
                            Semaforo = reader["Semaforo"] as string,
                            EventosDelDia = reader["EventosDelDia"] as string,
                            Observaciones = reader["Observaciones"] as string
                        };
                        lista.Add(dto);
                    }

                    // --- Segundo result set: resumen ---
                    if (await reader.NextResultAsync() && await reader.ReadAsync())
                    {
                        resumen = new ReporteJornadaResumenDto
                        {
                            TotalJornadas = reader["TotalJornadas"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalJornadas"]),
                            TotalUsuarios = reader["TotalUsuarios"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalUsuarios"]),
                            TotalHorasNetas = reader["TotalHorasNetas"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["TotalHorasNetas"]),
                            PromedioHorasNetas = reader["PromedioHorasNetas"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["PromedioHorasNetas"]),
                            PorcentajeCumplimientoPromedio = reader["PorcentajeCumplimientoPromedio"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["PorcentajeCumplimientoPromedio"]),
                            JornadasPuntuales = reader["JornadasPuntuales"] == DBNull.Value ? 0 : Convert.ToInt32(reader["JornadasPuntuales"]),
                            PorcentajePuntualidad = reader["PorcentajePuntualidad"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["PorcentajePuntualidad"])
                        };
                    }
                }
            }
            return (lista, resumen);
        }

    }
}
