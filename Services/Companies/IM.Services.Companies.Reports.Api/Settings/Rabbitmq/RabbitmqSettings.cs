namespace IM.Services.Companies.Reports.Api.Settings.Rabbitmq
{
    public class RabbitmqSettings : BaseServiceSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public RabbitmqQueueSettings QueueCompaniesReports { get; set; }
    }
    public class RabbitmqQueueSettings
    {
        public string Name { get; set; }
    }
}
