namespace Infrastructure.Interfaces.HappSpoofer;

public interface IHappSpoofer
{
    public Task<string> GetSubscriptionJson(string url, CancellationToken cancellationToken);
}