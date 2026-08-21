using System.Text;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UseCases.Queries;

namespace Controllers;

[ApiController]
[Route("sub")]
public class SubscriptionController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetSubscriptionJson(string id, CancellationToken cancellationToken)
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var hwid = Request.Headers["X-Hwid"].ToString();
        var subscription = await sender.Send(new GetSubJsonQuery(id, userAgent, hwid), cancellationToken);

        var titleBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(subscription.Title));
        Response.Headers.Add("profile-title", "base64:" + titleBase64);
        Response.Headers.Add("profile-update-interval", subscription.UpdateInterval.ToString());
        return new ContentResult
        {
            Content = subscription.Json,
            ContentType = "application/json"
        };
    }
}