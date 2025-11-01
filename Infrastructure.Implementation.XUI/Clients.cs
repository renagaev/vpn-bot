using Infrastructure.Interfaces.XUI;

namespace Infrastructure.Implementation.XUI;

public class ClientCollection
{
    public ICollection<ClientSettings> Clients { get; set; }
}