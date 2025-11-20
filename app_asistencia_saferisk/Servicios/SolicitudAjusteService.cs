using app_asistencia_saferisk.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace app_asistencia_saferisk.Servicios
{
    public class SolicitudAjusteService
    {

        private readonly AppAsistenciaDbContext _dbContext;
        public SolicitudAjusteService(AppAsistenciaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CrearSolicitudAsync(int? jornadaId, int usuarioId, string tipoAjuste, string descripcion)
        {
            using var conn = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_SolicitudAjuste_Crear", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@jornada_id", (object)jornadaId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@usuario_id", usuarioId);
            cmd.Parameters.AddWithValue("@tipo_ajuste", tipoAjuste);
            cmd.Parameters.AddWithValue("@descripcion", descripcion);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<List<SolicitudAjusteDto>> ListarPendientesAsync()
        {
            var lista = new List<SolicitudAjusteDto>();

            using var conn = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_SolicitudAjuste_ListarPendientes", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new SolicitudAjusteDto
                {
                    SolicitudId = reader.GetInt32(reader.GetOrdinal("solicitud_id")),
                    JornadaId = reader["jornada_id"] == DBNull.Value ? null : (int?)reader["jornada_id"],
                    UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                    NombreUsuario = reader["NombreUsuario"] as string,
                    TipoAjuste = reader["tipo_ajuste"] as string,
                    Descripcion = reader["descripcion"] as string,
                    Estado = reader["estado"] as string,
                    CreadoEl = (DateTime)reader["creado_el"],
                    FechaJornada = reader["FechaJornada"] == DBNull.Value ? null : (DateTime?)reader["FechaJornada"],
                    TipoJornada = reader["TipoJornada"] as string,
                    EstadoJornada = reader["EstadoJornada"] as string
                });
            }

            return lista;
        }

        public async Task MarcarAtendidoAsync(int solicitudId)
        {
            using var conn = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_SolicitudAjuste_MarcarAtendido", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@solicitud_id", solicitudId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<int> ContarPendientesAsync()
        {
            using var conn = new SqlConnection(_dbContext.Database.GetConnectionString());
            using var cmd = new SqlCommand("sp_SolicitudAjuste_ContarPendientes", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

    }
}
