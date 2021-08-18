namespace IM.Gateways.Web.Companies.Api.Settings.Mq
{
    public class Exchange
    {
        public string Name { get; set; } = null!;
        public string Type { get; set; } = "topic";
        public Queue[] Queues { get; set; } = null!;
    }
}
