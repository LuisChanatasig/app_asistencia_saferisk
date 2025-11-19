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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>

        /// <summary>
        /// Obtiene un reporte detallado de jornadas, un resumen total y un resumen mensual
        /// desde la base de datos utilizando ADO.NET y un procedimiento almacenado.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del rango del reporte.</param>
        /// <param name="fechaFin">Fecha de fin del rango del reporte.</param>
        /// <param name="usuarioId">ID opcional del usuario para filtrar el reporte.</param>
        /// <returns>Una tupla que contiene la lista de detalles de jornadas,
        /// el objeto de resumen total y la lista de resúmenes mensuales.</returns>
        public async Task<(List<ReporteJornadaDto> Detalles, ReporteJornadaResumenDto ResumenTotal, List<ReporteJornadaResumenMensualDto> ResumenMensual)> ObtenerReporteADOAsync(DateTime fechaInicio, DateTime fechaFin, int? usuarioId = null)
        {
            var listaDetalles = new List<ReporteJornadaDto>();
            ReporteJornadaResumenDto resumenTotal = null;
            var listaResumenMensual = new List<ReporteJornadaResumenMensualDto>();

            string connectionString = _dbContext.Database.GetConnectionString();

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_ReporteJornadaNormal", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fecha_inicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fecha_fin", fechaFin);

                // Manejo de usuarioId: si es 0, se pasa DBNull.Value (o null en SQL Server), de lo contrario se pasa el valor.
                // Esto es porque 0 podría ser un ID válido, pero usualmente en filtros significa "todos".
                // La lógica '@usuario_id IS NULL OR j.usuario_id = @usuario_id' en el SP maneja el NULL.
                cmd.Parameters.AddWithValue("@usuario_id", (object)(usuarioId == 0 ? null : usuarioId) ?? DBNull.Value);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // --- Primer conjunto de resultados: Detalles de Jornadas ---
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
                        listaDetalles.Add(dto);
                    }

                    // --- Segundo conjunto de resultados: Resumen Total ---
                    // Avanza al siguiente result set. NextResultAsync() devuelve true si hay más resultados.
                    if (await reader.NextResultAsync() && await reader.ReadAsync())
                    {
                        resumenTotal = new ReporteJornadaResumenDto
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

                    // --- Tercer conjunto de resultados: Resumen Mensual ---
                    // Vuelve a avanzar al siguiente result set.
                    if (await reader.NextResultAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var mensualDto = new ReporteJornadaResumenMensualDto
                            {
                                Mes = reader["Mes"] as string,
                                JornadasEnMes = reader["JornadasEnMes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["JornadasEnMes"]),
                                UsuariosActivosEnMes = reader["UsuariosActivosEnMes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["UsuariosActivosEnMes"]),
                                TotalHorasNetasMes = reader["TotalHorasNetasMes"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["TotalHorasNetasMes"]),
                                PromedioHorasNetasPorJornadaMes = reader["PromedioHorasNetasPorJornadaMes"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["PromedioHorasNetasPorJornadaMes"]),
                                PorcentajeCumplimientoPromedioMes = reader["PorcentajeCumplimientoPromedioMes"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["PorcentajeCumplimientoPromedioMes"]),
                                JornadasPuntualesEnMes = reader["JornadasPuntualesEnMes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["JornadasPuntualesEnMes"]),
                                PorcentajePuntualidadMes = reader["PorcentajePuntualidadMes"] == DBNull.Value ? null : (double?)Convert.ToDouble(reader["PorcentajePuntualidadMes"])
                            };
                            listaResumenMensual.Add(mensualDto);
                        }
                    }
                }
            }
            return (listaDetalles, resumenTotal, listaResumenMensual);
        }
        public async Task<IEnumerable<Usuario>> ListarUsuariosPorPerfilAsync(int perfilId, int usuarioId)
        {
            // Ejecuta el SP y mapea el resultado a la entidad Usuario
            return await _dbContext.Usuarios
                .FromSqlRaw(
                    "EXEC dbo.sp_ListarUsuariosPorPerfil @perfil_id = {0}, @usuario_id = {1}",
                    perfilId, usuarioId)
                .ToListAsync();
        }
    }
}
