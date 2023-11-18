using ChatApp.Data.Entites;

namespace ChatApp.Hubs.Interfaces
{
    public interface IChatingHubFunctions
    {
        public Task UserJoin(string name);
        public Task UserLeave(string name);
        public Task Clients(string clients_json);
        public Task GetClientOrGroup(string json);
        public Task Groups(string groups_json);
        public Task JoinToChat(string connectionId);
        public Task SendMessage(string message, bool fromGroup, string from, string groupName);
        public Task Error(string message);
    }
}
