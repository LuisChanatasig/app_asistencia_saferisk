using app_asistencia_saferisk.Models;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace app_asistencia_saferisk.Servicios
{
    public class ReporteService
    {
        private readonly AppAsistenciaDbContext _dbContext;
        public ReporteService(AppAsistenciaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ReporteJornadaDto>> ObtenerReporteAsync(DateTime fechaInicio, DateTime fechaFin, int? usuarioId = null)
        {
            var paramInicio = new SqlParameter("@fecha_inicio", fechaInicio);
            var paramFin = new SqlParameter("@fecha_fin", fechaFin);
            var paramUsuario = new SqlParameter("@usuario_id", usuarioId ?? (object)DBNull.Value);

            return await _dbContext.Set<ReporteJornadaDto>()
                .FromSqlRaw("EXEC sp_ReporteJornadaNormal @fecha_inicio, @fecha_fin, @usuario_id", paramInicio, paramFin, paramUsuario)
                .ToListAsync();
        }


    }
}
