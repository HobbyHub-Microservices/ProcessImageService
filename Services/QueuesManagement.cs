using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ImageProcessingService.Services.Interfaces;

namespace ImageProcessingService.Services;

public class QueuesManagement : IQueuesManagement
{
    public async Task<bool> SendMessage<T>(T serviceMessage, string queue, string connectionString)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(queue))
                throw new ArgumentException("Queue name cannot be null or empty.", nameof(queue));
        
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            
            await using var client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queue);
            

            if (serviceMessage == null)
                throw new ArgumentNullException(nameof(serviceMessage), "Service message cannot be null.");

            var msgBody = JsonSerializer.Serialize(serviceMessage);
            
            //Create a servicebus message
            ServiceBusMessage message = new ServiceBusMessage(msgBody);
            
            await sender.SendMessageAsync(message);
            
            return true;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Argument error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"Serialization error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An unexpected error occurred: {ex.Message}");
        }

        // Return false if an exception occurs
        return false;
    }
}