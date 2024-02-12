using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RabbitMQ;
[Serializable]
public class Message(string eventType, Dictionary<string, string> headers, Dictionary<string, string> payload, string sourceIp)
{
    public Guid MessageID { get; set; } = Guid.NewGuid();
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = eventType;
    public string SourceIP { get; set; } = sourceIp;
    public Dictionary<string, string> Headers { get; set; } = headers;
    public Dictionary<string, string> Payload { get; set; } = payload;
}
