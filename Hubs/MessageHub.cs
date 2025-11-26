using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;
namespace barangay_crime_compliant_api.Hubs
{
    public class MessageHub : Hub
    {
        // Thread-safe room membership tracking
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> ActiveRooms
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, byte>>();

        public async Task InitializeMessageRoom(string roomId)
        {
            string connectionId = Context.ConnectionId;

            // Create or add user to a thread-safe room
            var room = ActiveRooms.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, byte>());
            room.TryAdd(connectionId, 0);

            await Groups.AddToGroupAsync(connectionId, roomId);

            // Tell only the caller
            await Clients.Caller.SendAsync("MessageRoomInitialized", roomId);
        }

        public async Task SendMessage(string roomId,
            long messageId, long userId, long senderId, long receiverId,
            string message, string profileImage, string dateTimeNow)
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage",
                roomId, messageId, userId, senderId, receiverId,
                message, profileImage, dateTimeNow);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string connectionId = Context.ConnectionId;

            foreach (var room in ActiveRooms)
            {
                if (room.Value.ContainsKey(connectionId))
                {
                    room.Value.TryRemove(connectionId, out _);

                    // remove room if empty
                    if (room.Value.IsEmpty)
                    {
                        ActiveRooms.TryRemove(room.Key, out _);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public bool IsRoomActive(string roomId)
        {
            return ActiveRooms.ContainsKey(roomId);
        }
    }

}
