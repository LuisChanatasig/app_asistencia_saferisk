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
        /// Asynchronously retrieves a detailed workday report, including individual records, a total summary, and
        /// monthly summaries, filtered by the specified criteria.
        /// </summary>
        /// <remarks>This method executes the 'sp_ReporteJornadaNormal' stored procedure and returns
        /// multiple result sets. The operation is performed asynchronously and is suitable for scenarios where large
        /// report data may be retrieved. The returned data reflects the filters applied; omitting parameters broadens
        /// the scope of the report.</remarks>
        /// <param name="fechaInicio">The start date of the report period. If null, the current month is used as the default.</param>
        /// <param name="fechaFin">The end date of the report period. If null, the current month is used as the default.</param>
        /// <param name="usuarioId">The identifier of the user to filter the report. If null or 0, the report includes all users.</param>
        /// <param name="tipoJornada">The type of workday to filter by. If null or empty, all types are included.</param>
        /// <param name="estadoJornada">The workday status to filter by. If null or empty, all statuses are included.</param>
        /// <returns>A tuple containing a list of detailed workday records, a total summary, and a list of monthly summaries. If
        /// no data is found, the lists will be empty and the summary will contain zeroed values.</returns>
        public async Task<(
            List<ReporteJornadaDto> Detalles,
            ReporteJornadaResumenDto ResumenTotal,
            List<ReporteJornadaResumenMensualDto> ResumenMensual
        )> ObtenerReporteADOAsync(
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? usuarioId = null,
            string? tipoJornada = null,
            string? estadoJornada = null)
        {
            var listaDetalles = new List<ReporteJornadaDto>();
            ReporteJornadaResumenDto? resumenTotal = null;
            var listaResumenMensual = new List<ReporteJornadaResumenMensualDto>();

            var connectionString = _dbContext.Database.GetConnectionString();

            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_ReporteJornadaNormal", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Fechas: si son null, el SP aplica sus valores por defecto (mes actual)
            var pFechaInicio = cmd.Parameters.Add("@fecha_inicio", SqlDbType.Date);
            pFechaInicio.Value = (object?)fechaInicio ?? DBNull.Value;

            var pFechaFin = cmd.Parameters.Add("@fecha_fin", SqlDbType.Date);
            pFechaFin.Value = (object?)fechaFin ?? DBNull.Value;

            // usuario_id: null o 0 = todos
            var pUsuarioId = cmd.Parameters.Add("@usuario_id", SqlDbType.Int);
            if (usuarioId.HasValue && usuarioId.Value > 0)
                pUsuarioId.Value = usuarioId.Value;
            else
                pUsuarioId.Value = DBNull.Value;

            // tipo_jornada opcional
            var pTipoJornada = cmd.Parameters.Add("@tipo_jornada", SqlDbType.VarChar, 20);
            pTipoJornada.Value = string.IsNullOrWhiteSpace(tipoJornada)
                ? (object)DBNull.Value
                : tipoJornada;

            // estado_jornada opcional
            var pEstadoJornada = cmd.Parameters.Add("@estado_jornada", SqlDbType.VarChar, 20);
            pEstadoJornada.Value = string.IsNullOrWhiteSpace(estadoJornada)
                ? (object)DBNull.Value
                : estadoJornada;

            await conn.OpenAsync().ConfigureAwait(false);

            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // --- 1) Detalle de jornadas ---
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var dto = new ReporteJornadaDto
                {
                    JornadaId = reader.GetInt32(reader.GetOrdinal("JornadaId")),
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                    NombreUsuario = reader["NombreUsuario"] as string,
                    TipoJornada = reader["TipoJornada"] as string,
                    Fecha = GetNullableDateTime(reader, "Fecha"),
                    EstadoJornada = reader["EstadoJornada"] as string,

                    HoraEntrada = GetNullableTimeSpan(reader, "HoraEntrada"),
                    HoraSalida = GetNullableTimeSpan(reader, "HoraSalida"),
                    HoraCambioARemoto = GetNullableTimeSpan(reader, "HoraCambioARemoto"),
                    HoraCambioAOficina = GetNullableTimeSpan(reader, "HoraCambioAOficina"),

                    IpEntrada = reader["IpEntrada"] as string,
                    IpSalida = reader["IpSalida"] as string,
                    ValidaIpOficina = reader["ValidaIpOficina"] as string,

                    LatEntrada = GetNullableDecimal(reader, "LatEntrada"),
                    LonEntrada = GetNullableDecimal(reader, "LonEntrada"),
                    ValidaGpsOficina = reader["ValidaGpsOficina"] as string,
                    UbicacionOficina = reader["UbicacionOficina"] as string,

                    HorasBrutas = GetNullableDecimal(reader, "HorasBrutas"),
                    HorasEnRango = GetNullableDecimal(reader, "HorasEnRango"),

                    MinutosAlmuerzo = GetNullableInt(reader, "MinutosAlmuerzo"),
                    MinutosBreak = GetNullableInt(reader, "MinutosBreak"),
                    MinutosPermiso = GetNullableInt(reader, "MinutosPermiso"),
                    MinutosTraslado = GetNullableInt(reader, "MinutosTraslado"),
                    MinutosReunion = GetNullableInt(reader, "MinutosReunion"),
                    MinutosAtraso = GetNullableInt(reader, "MinutosAtraso"),
                    MinutosSalidaAnticipada = GetNullableInt(reader, "MinutosSalidaAnticipada"),

                    HorasNetas = GetNullableDecimal(reader, "HorasNetas"),
                    PorcentajeCumplimiento = GetNullableDecimal(reader, "PorcentajeCumplimiento"),

                    Puntualidad = reader["Puntualidad"] as string,
                    Semaforo = reader["Semaforo"] as string,

                    EventosDelDia = reader["EventosDelDia"] as string,
                    Observaciones = reader["Observaciones"] as string
                };

                listaDetalles.Add(dto);
            }

            // --- 2) Resumen total ---
            if (await reader.NextResultAsync().ConfigureAwait(false) &&
                await reader.ReadAsync().ConfigureAwait(false))
            {
                resumenTotal = new ReporteJornadaResumenDto
                {
                    TotalJornadas = GetInt(reader, "TotalJornadas"),
                    TotalUsuarios = GetInt(reader, "TotalUsuarios"),
                    TotalHorasNetas = GetNullableDecimal(reader, "TotalHorasNetas"),
                    PromedioHorasNetas = GetNullableDecimal(reader, "PromedioHorasNetas"),
                    PorcentajeCumplimientoPromedio = GetNullableDecimal(reader, "PorcentajeCumplimientoPromedio"),
                    JornadasPuntuales = GetInt(reader, "JornadasPuntuales"),
                    PorcentajePuntualidad = GetNullableDecimal(reader, "PorcentajePuntualidad")
                };
            }
            else
            {
                // Si no vino resumen (sin filas), devuelvo todo en cero
                resumenTotal = new ReporteJornadaResumenDto
                {
                    TotalJornadas = 0,
                    TotalUsuarios = 0,
                    TotalHorasNetas = null,
                    PromedioHorasNetas = null,
                    PorcentajeCumplimientoPromedio = null,
                    JornadasPuntuales = 0,
                    PorcentajePuntualidad = null
                };
            }

            // --- 3) Resumen mensual ---
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var mensualDto = new ReporteJornadaResumenMensualDto
                    {
                        Mes = reader["Mes"] as string,
                        JornadasEnMes = GetInt(reader, "JornadasEnMes"),
                        UsuariosActivosEnMes = GetInt(reader, "UsuariosActivosEnMes"),
                        TotalHorasNetasMes = GetNullableDecimal(reader, "TotalHorasNetasMes"),
                        PromedioHorasNetasPorJornadaMes = GetNullableDecimal(reader, "PromedioHorasNetasPorJornadaMes"),
                        PorcentajeCumplimientoPromedioMes = GetNullableDecimal(reader, "PorcentajeCumplimientoPromedioMes"),
                        JornadasPuntualesEnMes = GetInt(reader, "JornadasPuntualesEnMes"),
                        PorcentajePuntualidadMes = GetNullableDecimal(reader, "PorcentajePuntualidadMes")
                    };

                    listaResumenMensual.Add(mensualDto);
                }
            }

            return (listaDetalles, resumenTotal, listaResumenMensual);
        }


        /// <summary>
        /// Determines whether the specified field in the given data record contains a database null (DBNull) value.
        /// </summary>
        /// <param name="r">The data record to inspect for a database null value. Cannot be null.</param>
        /// <param name="name">The name of the field to check for a database null value. Cannot be null or empty.</param>
        /// <returns>true if the specified field contains a database null (DBNull) value; otherwise, false.</returns>

        private static bool IsDbNull(IDataRecord r, string name) =>
    r[name] == DBNull.Value;

        /// <summary>
        /// Retrieves the value of the specified field as an integer, returning 0 if the field is null.
        /// </summary>
        /// <remarks>If the field value cannot be converted to an integer, an exception may be thrown.
        /// This method treats database null values as 0.</remarks>
        /// <param name="r">The data record containing the field to retrieve.</param>
        /// <param name="name">The name of the field whose value is to be returned as an integer. Cannot be null.</param>
        /// <returns>The integer value of the specified field, or 0 if the field is null.</returns>
        private static int GetInt(IDataRecord r, string name) =>
            IsDbNull(r, name) ? 0 : Convert.ToInt32(r[name]);

        /// <summary>
        /// Retrieves the value of the specified field as a nullable 32-bit integer.
        /// </summary>
        /// <param name="r">The data record containing the field to retrieve.</param>
        /// <param name="name">The name of the field whose value is to be returned. Cannot be null.</param>
        /// <returns>A nullable 32-bit integer representing the value of the specified field, or null if the field is DBNull.</returns>
        private static int? GetNullableInt(IDataRecord r, string name) =>
            IsDbNull(r, name) ? (int?)null : Convert.ToInt32(r[name]);

        /// <summary>
        /// Retrieves the value of the specified field as a nullable decimal from the given data record.
        /// </summary>
        /// <param name="r">The data record containing the field to retrieve. Must not be null.</param>
        /// <param name="name">The name of the field to retrieve. Must not be null or empty.</param>
        /// <returns>A nullable decimal representing the value of the specified field, or null if the field is DBNull.</returns>
        private static decimal? GetNullableDecimal(IDataRecord r, string name) =>
            IsDbNull(r, name) ? (decimal?)null : Convert.ToDecimal(r[name]);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static DateTime? GetNullableDateTime(IDataRecord r, string name) =>
            IsDbNull(r, name) ? (DateTime?)null : Convert.ToDateTime(r[name]);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static TimeSpan? GetNullableTimeSpan(IDataRecord r, string name) =>
            IsDbNull(r, name) ? (TimeSpan?)null : (TimeSpan)r[name];


        /// <summary>
        /// 
        /// </summary>
        /// <param name="perfilId"></param>
        /// <param name="usuarioId"></param>
        /// <returns></returns>
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
