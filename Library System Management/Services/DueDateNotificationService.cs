using Library_System_Management.Repositories;

namespace Library_System_Management.Services
{
    // DueDateNotificationService: background worker that automatically runs the
    // due-date check when the app starts and then once every hour, creating
    // "due soon" and "fine accruing" notifications without any staff action.
    public class DueDateNotificationService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        private readonly ILibraryRepository _repo;
        private readonly ILogger<DueDateNotificationService> _logger;

        public DueDateNotificationService(ILibraryRepository repo, ILogger<DueDateNotificationService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    var created = _repo.GenerateDueDateNotifications(DateTime.UtcNow);
                    if (created > 0)
                    {
                        _logger.LogInformation("Due-date check created {Count} notification(s).", created);
                    }
                }
                catch (Exception ex)
                {
                    // a failed check should never stop the app; log it and try again next interval
                    _logger.LogError(ex, "Due-date notification check failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}