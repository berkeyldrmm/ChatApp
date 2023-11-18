using ChatApp.Data;
using ChatApp.Data.Entites;
using ChatApp.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Drawing;
using System.Text.Json;
using System.Xml.Linq;

namespace ChatApp.Hubs
{
    public class ChatingHub : Hub<IChatingHubFunctions>
    {
        public async Task addClient(string name, string color)
        {
            var client = new Client();
            client.Name = name;
            client.ConnectionId = Context.ConnectionId;
            client.Color = color;
            ClientSource.Clients.Add(client);
            await Clients.Caller.JoinToChat(Context.ConnectionId);
            await Clients.Others.UserJoin(ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId).Name);
            await Clients.All.Clients(JsonSerializer.Serialize(ClientSource.Clients));
        }

        public async Task addGroup(string groupName, string connectionIds_json)
        {
            Group group = new Group();
            group.GroupName = groupName;

            List<string> connectionIds = JsonSerializer.Deserialize<List<string>>(connectionIds_json);
            Client ownerOfGroup = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            group.OwnerOfGroup = ownerOfGroup;
            group.Clients.Add(ownerOfGroup);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            foreach (var connectionId in connectionIds)
            {
                Client? client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == connectionId);
                if (client != null)
                {
                    group.Clients.Add(client);
                    await Groups.AddToGroupAsync(connectionId, groupName);
                }
            }

            GroupSource.Groups.Add(group);
            var json = "[";
            foreach (var group_json in GroupSource.Groups)
            {
                var jsonGroup = JsonSerializer.Serialize(group_json);
                json += jsonGroup;
                json = json.Substring(0, json.Length - 1);
                json += ", \"clients\":";
                json += JsonSerializer.Serialize(group_json.Clients);
                json += "},";
            }
            json = json.Substring(0, json.Length - 1);
            json+= "]";

            await Clients.All.Groups(json);
        }

        public async Task getClient(string connectionId)
        {
            Client c = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == connectionId);
            await Clients.Caller.GetClientOrGroup(JsonSerializer.Serialize(c));
        }

        public async Task getGroup(string groupName)
        {
            Group g = GroupSource.Groups.FirstOrDefault(g => g.GroupName == groupName);
            await Clients.Caller.GetClientOrGroup(JsonSerializer.Serialize(g));
        }

        public async Task receiveMessage(string message, bool toGroup, string to)
        {
            try
            {
                if (toGroup)
                {
                    var from = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
                    await Clients.Group(to).SendMessage(message, true, JsonSerializer.Serialize(from), to);
                }
                else
                {
                    var from = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
                    await Clients.Client(to).SendMessage(message, false, JsonSerializer.Serialize(from), null);
                }
            }
            catch(Exception err)
            {
                await Clients.Caller.Error("Bir hata oluştu "+ err.Message);
            }
        }

        public override async Task OnConnectedAsync()
        {
            var client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (client != null)
            {
                ClientSource.Clients.Add(client);
                await Clients.Others.UserJoin(client.Name);
                await Clients.All.Clients(JsonSerializer.Serialize(ClientSource.Clients));
                await Clients.All.Groups(JsonSerializer.Serialize(GroupSource.Groups));
            }
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var client = ClientSource.Clients.FirstOrDefault(c => c.ConnectionId == Context.ConnectionId);
            if (client != null)
            {
                await Clients.Others.UserLeave(client.Name);
                ClientSource.Clients.Remove(client);
                await Clients.All.Clients(JsonSerializer.Serialize(ClientSource.Clients));
            }
        }
    }
}
