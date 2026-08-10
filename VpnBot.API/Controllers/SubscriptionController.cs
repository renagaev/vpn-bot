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
        var subscription = await sender.Send(new GetSubJsonQuery(id), cancellationToken);
        if (subscription == null)
            return NotFound();

        Response.Headers.Add("Profile-Title", "base64:" + Convert.ToBase64String(Encoding.UTF8.GetBytes(subscription.Title)));
        Response.Headers.Add("Profile-Update-Interval", subscription.UpdateInterval.ToString());
        return new ContentResult
        {
            Content = subscription.Json,
            ContentType = "application/json"
        };
    }
}