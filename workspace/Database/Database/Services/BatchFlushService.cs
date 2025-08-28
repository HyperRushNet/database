using Microsoft.Extensions.Hosting;

namespace Database.Services
{
    public class BatchFlushService : IHostedService
    {
        private readonly DatabaseService _database;

        public BatchFlushService(DatabaseService database)
        {
            _database = database;
        }

        public Task StartAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(System.Threading.CancellationToken cancellationToken)
        {
            _database.Dispose();
            return Task.CompletedTask;
        }
    }
}
