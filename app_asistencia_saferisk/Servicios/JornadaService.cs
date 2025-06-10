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
        public async Task<EstadoJornadaDto> ObtenerEstadoJornadaHoyAsync(int usuarioId)
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
                var jornadaId = reader.GetInt32(reader.GetOrdinal("jornada_id"));
                var tipoJornada = reader["tipo_jornada"]?.ToString();
                if (fecha.Date == hoy)
                    return new EstadoJornadaDto
                    {
                        Estado = "Presente",
                        PuedeRegistrar = false,
                        JornadaId = jornadaId,
                        TipoJornada = tipoJornada
                    };
                else
                    return new EstadoJornadaDto
                    {
                        Estado = "No iniciada",
                        PuedeRegistrar = true,
                        JornadaId = null,
                        TipoJornada = null
                    };
            }
            return new EstadoJornadaDto
            {
                Estado = "No iniciada",
                PuedeRegistrar = true,
                JornadaId = null,
                TipoJornada = null
            };
        }

        // 1.1 Obtener Estado de horas extra
        public async Task<EstadoHorasExtraDto> ObtenerEstadoHorasExtraHoyAsync(int usuarioId)
        {
            using var conn = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_jornadas_extra_abiertas_usuario", conn)
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
                var jornadaId = reader.GetInt32(reader.GetOrdinal("jornada_id"));
                var almuerzo = reader.GetString(reader.GetOrdinal("estado_almuerzo"));
                var estado = reader.GetString(reader.GetOrdinal("estado_horas_extra"));

                if (fecha.Date == hoy)
                    return new EstadoHorasExtraDto { Estado = estado, JornadaId = jornadaId, Almuerzo = almuerzo };
                else
                    return new EstadoHorasExtraDto { Estado = "noIniciado", JornadaId = null, Almuerzo = "noIniciado" };
            }
            return new EstadoHorasExtraDto { Estado = "noIniciado", JornadaId = null, Almuerzo = "noIniciado" };
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

        // 2.1 Registrar llegada horas extra (crea jornada y evento)
        public async Task<object> RegistrarLlegadaHorasExtraAsync(int usuarioId, string tipoJornada, string ip, decimal? latitud, decimal? longitud)
        {
            int jornadaId = 0;
            // 1. Registrar jornada (puedes pasar un flag o tipo especial, por ejemplo: "horas_extra")
            using (var conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_registrar_jornada", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
                cmd.Parameters.AddWithValue("@fecha", DateTime.Today.Date);
                cmd.Parameters.AddWithValue("@tipo_jornada", tipoJornada); // aquí llega "horas_extra"
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
                        return new { success = false, mensaje = "No se pudo registrar la jornada de horas extra." };
                    }
                }
                catch (SqlException ex)
                {
                    return new { success = false, mensaje = ex.Message };
                }
            }

            // 2. Registrar evento "horas_extra_inicio"
            using (var conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_registrar_evento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@jornada_id", jornadaId);
                cmd.Parameters.AddWithValue("@tipo_evento_codigo", "horas_extra_inicio");
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
                        return new { success = true, mensaje = "¡Horas extra registradas!" };
                    }
                    else
                    {
                        return new { success = false, mensaje = "No se pudo registrar el evento de horas extra." };
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
                        "break_inicio" => "mdi-coffee-outline text-info",
                        "break_fin" => "mdi-coffee text-success",
                        "remoto" => "mdi-home-city-outline text-info",
                        "oficina" => "mdi-office-building-marker-outline text-primary",
                        "permiso" => "mdi-account-cancel text-warning",
                        "permiso_fin" => "mdi-account-check text-success",
                        "reunion" => "mdi-account-group text-secondary",
                        "soporte técnico visita" => "mdi-laptop-wrench text-secondary",
                        "otro" => "mdi-dots-horizontal text-dark",
                        "traslado_inicio" => "mdi-car-arrow-right text-info",
                        "traslado_fin" => "mdi-flag-checkered text-success",
                        // Puedes seguir agregando más según tu tabla tipo_evento
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
                        codigo = reader["tipo_evento"].ToString(),  // <-- IMPORTANTE para los botones!

                        descripcion = reader["descripcion"].ToString(),
                        icono,
                        hora = horaStr,
                        obs = reader["observaciones"]?.ToString() ?? ""
                    });

                }
            }
            return timeline;
        }

        // 4. Registrar eventos
        public async Task<(bool success, string mensaje)> RegistrarEventoAsync(
        int jornadaId,
        string tipoEventoCodigo,
        string observaciones,
        string dispositivo,
        string ip,
        decimal? latitud,
        decimal? longitud)
        {
            using var conn = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_registrar_evento", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@jornada_id", jornadaId);
            cmd.Parameters.AddWithValue("@tipo_evento_codigo", tipoEventoCodigo);
            cmd.Parameters.AddWithValue("@observaciones", (object)observaciones ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dispositivo", (object)dispositivo ?? DBNull.Value);
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
                    return (true, "Evento registrado correctamente.");
                }
                else
                {
                    return (false, "No se pudo registrar el evento.");
                }
            }
            catch (SqlException ex)
            {
                return (false, ex.Message);
            }
        }


        //4.1 Registrar eventos Horas extra

        public async Task<object> RegistrarEventoHorasExtraAsync(int usuarioId, string tipoEvento, string observaciones, string ip, decimal? latitud, decimal? longitud)
        {
            int jornadaId = 0;

            // 1. Obtener la jornada extra abierta de hoy
            using (var conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_jornadas_extra_abiertas_usuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@usuario_id", usuarioId);

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    jornadaId = Convert.ToInt32(reader["jornada_id"]);
                }
                else
                {
                    return new { success = false, mensaje = "No hay jornada extra abierta hoy. Inicia primero las horas extra." };
                }
            }

            // 2. Registrar evento
            using (var conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_registrar_evento", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@jornada_id", jornadaId);
                cmd.Parameters.AddWithValue("@tipo_evento_codigo", tipoEvento);
                cmd.Parameters.AddWithValue("@observaciones", string.IsNullOrEmpty(observaciones) ? DBNull.Value : (object)observaciones);
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
                        return new { success = true, mensaje = "Evento registrado correctamente." };
                    }
                    else
                    {
                        return new { success = false, mensaje = "No se pudo registrar el evento." };
                    }
                }
                catch (SqlException ex)
                {
                    return new { success = false, mensaje = ex.Message };
                }
            }
        }



        //5. Cerrar Jornada
        public async Task<object> CerrarJornadaAsync(int jornadaId)
        {
            using (var conn = new SqlConnection(_dbContext.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_cerrar_jornada", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@jornada_id", jornadaId);

                try
                {
                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync() && reader["estado"].ToString() == "OK")
                    {
                        return new { success = true, mensaje = "Jornada cerrada correctamente.", jornadaId = (int)reader["jornada_id"] };
                    }
                    else
                    {
                        return new { success = false, mensaje = "No se pudo cerrar la jornada." };
                    }
                }
                catch (SqlException ex)
                {
                    return new { success = false, mensaje = ex.Message };
                }
            }
        }
    }
}
