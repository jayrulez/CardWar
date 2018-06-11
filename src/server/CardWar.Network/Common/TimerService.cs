using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace CardWar.Network.Common
{
    public class TimerService
    {
        private Stopwatch stopwatch = new Stopwatch();

        private readonly ILogger _logger;

        public TimerService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TimerService>();
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }
            else
            {
                _logger.LogWarning("The stopwatch is already running.");
            }
        }

        public long GetTicks()
        {
            if (!stopwatch.IsRunning)
            {
                throw new Exception("The stopwatch is not running.");
            }

            return stopwatch.ElapsedTicks;
        }

        public void Stop()
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
        }
    }
}