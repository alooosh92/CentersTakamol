using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace CentersTakamol.Models
{
    internal class SendReceipt
    {
        public ChatId? Id { get; set; }
        public string? RemittanceNum { get; set; }
        public string? ImageName { get; set; }
        public int? BookNumber { get; set; }
        public int? Price { get; set; }
        public int Count { get; set; } = 0;
        public string? ErorrPriceNote { get; set; } = "";
        public List<Receipt>? Receipts { get; set; }
    }
}
