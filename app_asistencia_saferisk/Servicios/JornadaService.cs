using app_asistencia_saferisk.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace app_asistencia_saferisk.Servicios
{
    public class JornadaService
    {
        private readonly AppAsistenciaDbContext _dbContext;
        public JornadaService(AppAsistenciaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // 1. Estado actual
        public async Task<object> ObtenerEstadoJornadaHoyAsync(int usuarioId)
        {
            using var conn = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_jornadas_abiertas_usuario", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var fecha = reader.GetDateTime(reader.GetOrdinal("fecha"));
                var hoy = DateTime.Today;
                if (fecha.Date == hoy)
                    return new { estado = "Presente", puedeRegistrar = false };
                else
                    return new { estado = "No iniciada", puedeRegistrar = true };
            }
            return new { estado = "No iniciada", puedeRegistrar = true };
        }

        // 2. Registrar llegada (crea jornada y evento)
        public async Task<object> RegistrarLlegadaAsync(int usuarioId, string tipoJornada, string ip, decimal? latitud,decimal? longitud)
        {
            int jornadaId = 0;
            // Primero, registrar jornada
            using (var conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_registrar_jornada", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Today.Date);
                cmd.Parameters.AddWithValue("@tipo_jornada", tipoJornada);
                cmd.Parameters.AddWithValue("@observaciones", DBNull.Value);

                var outId = new SqlParameter("@jornada_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(outId);

                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync() && reader["estado"].ToString() == "OK")
                    {
                        jornadaId = Convert.ToInt32(reader["jornada_id"]);
                    }
                    else
                    {
                        return new { success = false, mensaje = "No se pudo registrar la jornada." };
                    }
                }
                catch (SqlException ex)
                {
                    return new { success = false, mensaje = ex.Message };
                }
            }

            // Luego, registrar evento "entrada"
            using (var conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_registrar_evento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@jornada_id", jornadaId);
                cmd.Parameters.AddWithValue("@tipo_evento_codigo", "entrada");
                cmd.Parameters.AddWithValue("@observaciones", tipoJornada);
                cmd.Parameters.AddWithValue("@dispositivo", "Web");
                cmd.Parameters.AddWithValue("@ip_registro", (object)ip ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@latitud", (object)latitud ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@longitud", (object)longitud ?? DBNull.Value);
                var outId = new SqlParameter("@evento_id", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(outId);
                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync() && reader["estado"].ToString() == "OK")
                    {
                        return new { success = true, mensaje = "¡Llegada registrada!" };
                    }
                    else
                    {
                        return new { success = false, mensaje = "No se pudo registrar el evento de entrada." };
                    }
                }
                catch (SqlException ex)
                {
                    return new { success = false, mensaje = ex.Message };
                }
            }
        }

        // 3. Timeline de hoy
        public async Task<List<object>> ObtenerTimelineHoyAsync(int usuarioId)
        {
            var timeline = new List<object>();
            using (var conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_timeline_usuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Today);

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var codigo = reader["tipo_evento"].ToString();
                    var icono = codigo switch
                    {
                        "entrada" => "mdi-login-variant text-success",
                        "salida" => "mdi-logout-variant text-danger",
                        "almuerzo_inicio" => "mdi-silverware-fork-knife text-warning",
                        "almuerzo_fin" => "mdi-food text-success",
                        "remoto" => "mdi-home-city-outline text-info",
                        "oficina" => "mdi-office-building-marker-outline text-primary",
                        _ => "mdi-checkbox-blank-circle-outline text-secondary"
                    };

                    string horaStr = "";
                    if (reader["hora_registro"] != DBNull.Value)
                    {
                        var raw = reader["hora_registro"];
                        if (raw is TimeSpan ts)
                            horaStr = ts.ToString(@"hh\:mm");
                        else
                            horaStr = TimeSpan.Parse(raw.ToString()).ToString(@"hh\:mm");
                    }

                    timeline.Add(new
                    {
                        descripcion = reader["descripcion"].ToString(),
                        icono,
                        hora = horaStr,
                        obs = reader["observaciones"]?.ToString() ?? ""
                    });

                }
            }
            return timeline;
        }
    }
}
