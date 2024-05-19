using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentersTakamol.Models
{
    internal class RegionSupervisor
    {
        public string? Region { get; set; }
        public MailboxAddress? Supervisor { get; set; }
    }
}
