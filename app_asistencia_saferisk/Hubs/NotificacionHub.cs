using Microsoft.AspNetCore.SignalR;

namespace app_asistencia_saferisk.Hubs
{
    public class NotificacionHub : Hub
    {
        // Cada vez que un cliente se conecta al hub
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext?.Session.GetInt32("UsuarioId");

            if (userId != null)
            {
                // Agregamos esta conexión al grupo del usuario
                await Groups.AddToGroupAsync(Context.ConnectionId, $"usuario_{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext?.Session.GetInt32("UsuarioId");

            if (userId != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"usuario_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
