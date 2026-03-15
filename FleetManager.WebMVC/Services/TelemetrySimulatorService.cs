using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FleetManager.Domain.Models;
using FleetManager.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FleetManager.WebMVC.Services
{
    public class TelemetrySimulatorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TelemetrySimulatorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                int delayMs = random.Next(5000, 180001);

                try
                {
                    await Task.Delay(delayMs, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<FleetContext>();

                    var robots = await context.Robots.ToListAsync(stoppingToken);
                    var severities = await context.LogSeverities
                                        .Where(s => s.Name.ToLower() != "smth")
                                        .ToListAsync(stoppingToken);

                    if (robots.Any() && severities.Any())
                    {
                        var mockMessages = new[] {
                            "[AUTO] Temperature spike in joint 3",
                            "[AUTO] Vision system latency > 50ms",
                            "[AUTO] Calibration required",
                            "[AUTO] Network signal weak",
                            "[AUTO] Battery level at 15%",
                            "[AUTO] Object successfully grasped"
                        };

                        int logsToGenerate = random.Next(1, 3);
                        for (int i = 0; i < logsToGenerate; i++)
                        {
                            var newLog = new HardwareLog
                            {
                                RobotId = robots[random.Next(robots.Count)].Id,
                                SeverityId = severities[random.Next(severities.Count)].Id,
                                Message = mockMessages[random.Next(mockMessages.Length)]
                            };
                            context.Add(newLog);
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }
            }
        }
    }
}