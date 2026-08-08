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
        var json = await sender.Send(new GetSubJsonQuery(id), cancellationToken);
        if (json == null)
            return NotFound();

        return new ContentResult
        {
            Content = json,
            ContentType = "application/json"
        };
    }
}