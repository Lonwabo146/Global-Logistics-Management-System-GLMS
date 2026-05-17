using GLMS.Models;
using Microsoft.Extensions.Logging;

namespace GLMS.Services.Observer
{
    public class NotificationService : IContractObserver
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task Update(Contract contract, string changeDescription)
        {
            _logger.LogInformation(
                "[NOTIFICATION] Contract ID {ContractId} for Customer ID " +
                "{CustomerId} — {Change} at {Time}",
                contract.ContractId,
                contract.CustomerId,
                changeDescription,
                DateTime.Now);

            return Task.CompletedTask;
        }
    }
}