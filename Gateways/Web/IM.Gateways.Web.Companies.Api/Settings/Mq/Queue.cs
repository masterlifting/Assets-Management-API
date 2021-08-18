namespace IM.Gateways.Web.Companies.Api.Settings.Mq
{
    public class Queue
    {
        public string Name { get; set; } = null!;
        public QueueParam[] Params { get; set; } = null!;
    }
}
