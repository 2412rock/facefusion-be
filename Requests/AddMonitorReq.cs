using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Uptime_Monitor_Backend.Models.Requests
{
    public class AddMonitorReq
    {
        public string MonitorName { get; set; }
        public string MonitorType { get; set; } // Http/https/ping/ws/wss
        public string? HttpMethod { get; set; }
        public bool CheckCertificate { get; set; }
        public string Url { get; set; }
        public int CheckInterval { get; set; }
    }

    public class EditMonitorReq
    {
        public int Id { get; set; }
        public string MonitorName { get; set; }
        public string MonitorType { get; set; } // Http/https/ping/ws/wss
        public string? HttpMethod { get; set; }
        public bool CheckCertificate { get; set; }
        public string Url { get; set; }
        public int CheckInterval { get; set; }
    }
}
