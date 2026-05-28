using Microsoft.AspNetCore.SignalR;

namespace ShowSphere.API.Hubs;

public class SeatHub : Hub
{
    public async Task JoinShowGroup(string showId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"show_{showId}");
    }

    public async Task LeaveShowGroup(string showId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"show_{showId}");
    }

    public async Task NotifySeatUpdate(string showId, string seatId, bool isAvailable)
    {
        await Clients.Group($"show_{showId}").SendAsync("SeatUpdated", seatId, isAvailable);
    }
}
