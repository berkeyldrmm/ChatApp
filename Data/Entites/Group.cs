namespace ChatApp.Data.Entites
{
    public class Group
    {
        public string GroupName { get; set; }
        public readonly List<Client> Clients = new List<Client>();
        public Client OwnerOfGroup { get; set; }
    }
}
