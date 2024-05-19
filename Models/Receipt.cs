using Telegram.Bot.Types;

namespace CentersTakamol.Models
{
    internal class Receipt
    {
        public ChatId? Id { get; set; }
        public string? Region { get; set; }
        public string? CenterName { get; set; }
        public int? FirstReceipt { get; set; }
        public int? LastReceipt { get; set; }
        public int FamilyReceiptNum { get; set; }
        public int VecationReceiptNum { get; set; }
        public int DamagedReceiptNum { get; set; }
        public List<int>? FamilyReceiptList { get; set; }
        public List<int>? VactionReceiptList { get; set; }
        public List<int>? DamagedReceiptList { get; set; }
        public string? Notes { get; set; }
        public int Count { get; set; } = 0;
    }
}
