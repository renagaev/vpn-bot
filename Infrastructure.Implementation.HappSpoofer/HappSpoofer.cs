using Infrastructure.Interfaces.HappSpoofer;

namespace Infrastructure.Implementation.HappSpoofer;

internal class HappSpoofer(HttpClient httpClient) : IHappSpoofer
{
    public async Task<string> GetSubscriptionJson(string url, CancellationToken cancellationToken)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("User-Agent", "Happ/3.14.0");
        req.Headers.Add("X-Device-Os", "Android");
        req.Headers.Add("X-Device-Locale", "ru");
        req.Headers.Add("X-Device-Model", "ELP-NX1");
        req.Headers.Add("X-Ver-Os", "16");
        // req.Headers.Add("Accept-Encoding", "gzip");
        req.Headers.Add("Connection", "close");
        req.Headers.Add("X-Hwid", "HWD-DFFF-SDE4D");
        req.Headers.Add("X-Real-Ip", "213.135.154.127");
        req.Headers.Add("X-Forwarded-For", "213.135.154.127");

        var response = await httpClient.SendAsync(req, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}