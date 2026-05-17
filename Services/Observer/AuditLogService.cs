using GLMS.Models;
using Microsoft.Extensions.Logging;

namespace GLMS.Services.Observer
{
    public class AuditLogService : IContractObserver
    {
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(ILogger<AuditLogService> logger)
        {
            _logger = logger;
        }

        public Task Update(Contract contract, string changeDescription)
        {
            _logger.LogWarning(
                "[AUDIT LOG] Contract ID {ContractId} | Status: {Status} " +
                "| Change: {Change} | Recorded: {Time}",
                contract.ContractId,
                contract.Status,
                changeDescription,
                DateTime.Now);

            return Task.CompletedTask;
        }
    }
}