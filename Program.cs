using CentersTakamol.Controller;
using CentersTakamol.Data;
using CentersTakamol.Models;
using System.Text;
using System.Text.Json;
using Telegram.Bot;

namespace CentersTakamol
{
    internal class Program
    {
        public async static void AddFile()
        {

            if (!File.Exists("RegionCenter.json"))
            {
                await using FileStream createStream = File.Create("RegionCenter.json");
                createStream.Close();
            }
            if (!File.Exists("UserBot.json"))
            {
                await using FileStream createStream = File.Create("UserBot.json");
                createStream.Close();
            }
            if (!File.Exists("AdminBot.json"))
            {
                await using FileStream createStream = File.Create("AdminBot.json");
                createStream.Close();
            }
            if (!File.Exists("SuperAdminBot.json"))
            {
                await using FileStream createStream = File.Create("SuperAdminBot.json");
                createStream.Close();
                AddSuperAdmin sa = new AddSuperAdmin
                {
                    Id = 594893404,
                    Name = "علاء بعاج"
                };
                var readFile = File.ReadLines("SuperAdminBot.json").ToList();
                readFile.Add(JsonSerializer.Serialize(sa, typeof(AddSuperAdmin), new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) }));
                await File.WriteAllLinesAsync("SuperAdminBot.json", readFile);
            }
            if (!File.Exists("RegionBot.json"))
            {
                await using FileStream createStream = File.Create("RegionBot.json");
                createStream.Close();
            }
        }
        static void Main(string[] args)
        {
            AddFile();
            List<Receipt> receipts = new List<Receipt>();
            List<SendReceipt> sendeReceipts = new List<SendReceipt>();
            List<UserBot> bots = new List<UserBot>();
            var botClient = new TelegramBotClient(Values.TelegramBotId);
            Console.OutputEncoding = Encoding.UTF8;
            ReceintController receintController = new ReceintController(botClient,receipts,sendeReceipts,bots);
            receintController.ConntionToBot();
            Console.ReadLine();
        }
    }
}