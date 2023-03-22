using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Minmaxdev.Common.Services
{
    /// <summary>
    /// Use to wrap a function call elapsed time into a logging message
    /// </summary>
    public class StopwatchWrapper
    {
        private readonly Serilog.ILogger logger;

        public StopwatchWrapper(
            Serilog.ILogger logger
            )
        {
            this.logger = logger;
        }

        public void RunAndLog(string name, Action doWork, LogLevel logLevel = LogLevel.Debug)
        {
            var watchLoop = Stopwatch.StartNew();
            doWork();
            watchLoop.Stop();

            var strTime = (watchLoop.ElapsedMilliseconds / 1000d).ToString("#,##0.####");

            switch (logLevel)
            {
                case LogLevel.Critical:
                    logger.Fatal("{name} completed. Elapsed: {elapsedSeconds}s", name, strTime);
                    break;

                case LogLevel.Error:
                    logger.Error("{name} completed. Elapsed: {elapsedSeconds}s", name, strTime);
                    break;

                case LogLevel.Warning:
                    logger.Warning("{name} completed. Elapsed: {elapsedSeconds}s", name, strTime);
                    break;

                case LogLevel.Information:
                    logger.Information("{name} completed. Elapsed: {elapsedSeconds}s", name, strTime);
                    break;

                default:
                    logger.Debug("{name} completed. Elapsed: {elapsedSeconds}s", name, strTime);
                    break;
            };
        }

        public T RunAndLog<T>(string name, Func<T> doWork)
        {
            T result = default;
            RunAndLog(name, (Action)(() => result = doWork()));
            return result;
        }
    }
}