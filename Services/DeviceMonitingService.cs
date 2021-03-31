using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IntegrationNxWitness.Services
{
    public class DeviceMonitingService : BackgroundService
    {
        private readonly ILogger<DeviceMonitingService> _logger;

        public DeviceMonitingService(ILogger<DeviceMonitingService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(DateTime.Now.ToString());
                await Task.Delay(1000);
            }

            
        }
    }
}