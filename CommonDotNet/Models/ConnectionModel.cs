namespace CommonServices.Models
{
    public abstract class ConnectionModel
    {
        public string Server { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
