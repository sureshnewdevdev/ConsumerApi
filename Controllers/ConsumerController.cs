using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class ConsumerController : ControllerBase
{
    private readonly string _connectionString;
    private readonly string _queueName;

    public ConsumerController(IConfiguration configuration)
    {
        _connectionString = configuration.GetSection("ServiceBus:ConnectionString").Value;
        _queueName = configuration.GetSection("ServiceBus:QueueName").Value;
    }

    [HttpGet]
    public async Task<IActionResult> ReceiveMessages()
    {
        await using var client = new ServiceBusClient(_connectionString);
        ServiceBusReceiver receiver = client.CreateReceiver(_queueName);

        try
        {
            ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync();

            if (message != null)
            {
                string messageBody = message.Body.ToString();
                await receiver.CompleteMessageAsync(message);
                return Ok(messageBody);
            }
            else
            {
                return Ok("No messages available");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
