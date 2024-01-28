using Chatapplication.Model;
using Microsoft.AspNetCore.SignalR;

namespace Chatapplication.HUB
{
    public class ChatHUB : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IDictionary<string, UserRoomConnection> _connection;
        public ChatHUB(IDictionary<string,UserRoomConnection> connection)
        {
            _connection= connection;
        }
        public async Task JoinRoom(UserRoomConnection userRoomConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userRoomConnection.room!);
            _connection[Context.ConnectionId] = userRoomConnection;
            await Clients.Group(userRoomConnection.room!)
                .SendAsync("Recieved message", "amrish's bot", $"{userRoomConnection.user} has joined the group");
            await SendConnectedUser(userRoomConnection.room!);
        }

        public async Task SendMessage(string message)
        {
            if (_connection.TryGetValue(Context.ConnectionId,out UserRoomConnection userRoomConnection))
            {
                await Clients.Group(userRoomConnection.room!)
                    .SendAsync("Recieved message", userRoomConnection.user, message,DateTime.Now);
            }
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (!_connection.TryGetValue(Context.ConnectionId,out UserRoomConnection userRoomConnection))
            {
                return base.OnDisconnectedAsync(exception);
            }
            Clients.Group(userRoomConnection.room!).SendAsync("RecievedMessage", userRoomConnection.room,$"{ userRoomConnection.user} has left the group");
            SendConnectedUser(userRoomConnection.room!);
            return base.OnDisconnectedAsync(exception);
        }
        public Task SendConnectedUser(string room)
        {
            var users = _connection.Values
                .Where(u => u.room == room)
                .Select(s => s.user);
            return Clients.Group(room).SendAsync("ConnectedUser", users);
        }

    } 
}
