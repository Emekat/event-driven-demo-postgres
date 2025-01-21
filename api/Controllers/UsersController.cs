using api.DTO;
using Microsoft.AspNetCore.Mvc;
using Play.Messaging.Events;
using Play.Models;
using Play.PostgresEventStore;

namespace api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IEventStore eventStore) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserRequest request)
    {
        //User created here
        var user = new User(Guid.CreateVersion7().ToString(), request.TenantId, request.Name);
        
        var aggregateId = Guid.CreateVersion7().ToString();
        
        var userRegisteredEvent =
            new UserRegisteredEvent(aggregateId, 1, user.UserId,  user.TenantId, user.Name);

        await eventStore.SaveEventsAsync(aggregateId, [userRegisteredEvent], -1);

        return Ok(new { user.UserId });
    }
}