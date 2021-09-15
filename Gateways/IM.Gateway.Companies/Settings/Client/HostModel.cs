﻿namespace IM.Gateway.Companies.Settings.Client
{
    public abstract class HostModel
    {
        public string Schema { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string Controller { get; set; } = null!;
    }
}
