using System.Diagnostics;
using System.Threading.Tasks;
using MTGAHelper.Server.DataAccess;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Web.UI.Helpers
{
    public class LoggingQueryHandlerDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        private readonly IQueryHandler<TQuery, TResult> decoratee;
        private readonly ILogger logger;

        public LoggingQueryHandlerDecorator(IQueryHandler<TQuery, TResult> decoratee, ILogger logger)
        {
            this.decoratee = decoratee;
            this.logger = logger;
        }

        public async Task<TResult> Handle(TQuery query)
        {
            var sw = new Stopwatch();
            sw.Start();
            var result = await decoratee.Handle(query);
            sw.Stop();

            logger.Debug($"Query took {sw.ElapsedMilliseconds}ms: {typeof(TQuery).Name} {{query}}", JsonConvert.SerializeObject(query));

            return result;
        }
    }
}
