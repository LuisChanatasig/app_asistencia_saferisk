using app_asistencia_saferisk.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Data;

namespace app_asistencia_saferisk.Servicios
{
    public class AutenticacionService
    {
        private readonly AppAsistenciaDbContext _dbContex;

        public AutenticacionService (AppAsistenciaDbContext dbContex)
        {
            _dbContex = dbContex ?? throw new ArgumentNullException(nameof(dbContex));
        }

        // Valida el usuario usando el SP
        public async Task<LoginResponse> ValidarUsuarioAsync(string ci, string pass)
        {
            using (var conn = new SqlConnection(_dbContex.Database.GetConnectionString()))
            using (var cmd = new SqlCommand("sp_validar_usuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ci", ci);
                cmd.Parameters.AddWithValue("@pass", pass);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new LoginResponse
                        {
                            UsuarioId = reader.GetInt32(reader.GetOrdinal("usuario_id")),
                            Nombres = reader.GetString(reader.GetOrdinal("nombres")),
                            RolId = reader.GetInt32(reader.GetOrdinal("rol_id")),
                            Estado = reader.GetInt32(reader.GetOrdinal("estado")),
                            SupervisorId = reader.IsDBNull(reader.GetOrdinal("supervisor_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("supervisor_id"))
                        };
                    }
                }
            }
            return null;
        }
    }
}
