using CentersTakamol.Controller;
using CentersTakamol.Data;
using CentersTakamol.Models;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CentersTakamol
{
    internal class ReceintController
    {
        public ReceintController(TelegramBotClient telegramBotClient, List<Receipt> receipts, List<SendReceipt> sendReceipts, List<UserBot> userBots)
        {
            TelegramBotClient = telegramBotClient;
            Receipts = receipts;
            SendReceipts = sendReceipts;
            UserBots = userBots;
        }
        private TelegramBotClient TelegramBotClient { get; }
        private List<Receipt> Receipts { get; }
        private List<SendReceipt> SendReceipts { get; }
        private List<UserBot> UserBots { get; }
        bool AddRegion = false;
        bool RemoveRegion = false;
        bool AddSuperAdmin = false;
        bool DeleteSuperAdmin = false;
        bool AddCenter = false;
        bool DeleteCenter = false;
        AdminBot? AddAdmin = null;
        bool DeleteAdmin = false;
        public async void ConntionToBot()
        {
            using CancellationTokenSource cts = new();
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            TelegramBotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
                );
            var me = await TelegramBotClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            cts.Cancel();
        }
        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        private void AddChoiceButton(ChatId chatId, string messageText, List<string> listOption)
        {
            var list = new KeyboardButton[listOption.Count + 1][];
            for (int i = 0; i < listOption.Count; i++)
            {
                list[i] = new[] { new KeyboardButton(listOption[i]) };
            }
            list.SetValue(new[] { new KeyboardButton("العودة إلى البداية") }, listOption.Count);
            var replyKeyboardMarkup = new ReplyKeyboardMarkup(list);
            replyKeyboardMarkup.ResizeKeyboard = true;
            var message = messageText;
            TelegramBotClient!.SendTextMessageAsync(chatId, message, replyMarkup: replyKeyboardMarkup);
        }
        private async Task<int?> IsNumber(ChatId chatId, string number, CancellationToken cancellationToken)
        {
            try
            {
                return int.Parse(number);
            }
            catch (FormatException)
            {
                await TelegramBotClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: $"{number}: ليس رقم صحيح الرجاء ادخال رقم صحيح",
                 cancellationToken: cancellationToken);
                return null;
            }

        }
        private bool Receipt50(Receipt receipt)
        {
            int n = receipt.VecationReceiptNum + receipt.FamilyReceiptNum + receipt.DamagedReceiptNum;
            if (n > 50) { return false; }
            return true;
        }
        private async Task<bool> IsRemittanceNumber(ChatId chatId, string RemittanceNumber, CancellationToken cancellationToken)
        {
            if (!Regex.IsMatch(RemittanceNumber, @"\d{4}-\d{4}-\d{4}"))
            {
                await TelegramBotClient.SendTextMessageAsync(
                 chatId: chatId,
                 text: "الرجاء ادخال رقم الحوالة صحيح على الشكل: xxxx-xxxx-xxxx",
                 cancellationToken: cancellationToken);
                return false;
            }
            return true;
        }
        private async Task<List<int>> ListReceiptNumber(ChatId chatId, string text, int? itemNumber, CancellationToken cancellationToken)
        {
            if (itemNumber != null && itemNumber > 1)
            {
                char[] ch = { ',', '-', '/', '_', '~' };
                foreach (var c in ch)
                {
                    if (text.StartsWith(c))
                    {
                        text = text.Substring(1);
                    }
                }
                int n = text.Split(ch).Length;
                List<int> ret = new List<int>();
                if (n == itemNumber)
                {
                    foreach (string i in text.Split(ch))
                    {
                        try
                        {
                            int nu = int.Parse(i);
                            if (!ret.Contains(nu))
                            {
                                ret.Add(nu);
                            }
                            else
                            {
                                await TelegramBotClient.SendTextMessageAsync(
                                 chatId: chatId,
                                 text: $"رقم الايصال {nu} مكرر",
                                 cancellationToken: cancellationToken);
                                return new();
                            }
                        }
                        catch
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                                 chatId: chatId,
                                 text: $"الايصال {i} مدخل بطريقة خاطئة",
                                 cancellationToken: cancellationToken);
                            return new();
                        }

                    }
                }
                else
                {
                    await TelegramBotClient.SendTextMessageAsync(
                                 chatId: chatId,
                                 text: "عدد الايصالات غير مطابق الرجاء اعادة ادخال الايصالات بطريقة صحيحة",
                                 cancellationToken: cancellationToken);
                    return new();
                }
                return ret;
            }
            else
            {
                try
                {
                    return new List<int> { int.Parse(text) };
                }
                catch
                {
                    await TelegramBotClient.SendTextMessageAsync(
                         chatId: chatId,
                         text: $"رقم الايصال مدخل بطريقة خاطئة",
                         cancellationToken: cancellationToken);
                    return new();
                }
            }
        }
        async Task ShowBook(SendReceipt book, CancellationToken cancellationToken)
        {
            string erorrPrice = "";
            if (book.ErorrPriceNote != String.Empty)
            {
                erorrPrice = $"خطأ بقيمة الحوالة:\n{book.ErorrPriceNote}";
            }
            string booksInfo = "";
            foreach (var Rec in book.Receipts!)
            {
                string family = "";
                string vecation = "";
                string damaged = "";
                foreach (var item in Rec.FamilyReceiptList!)
                {
                    family += $"{item} /";
                }
                foreach (var item in Rec.VactionReceiptList!)
                {
                    vecation += $"{item} /";
                }
                foreach (var item in Rec.DamagedReceiptList!)
                {
                    damaged += $"{item} /";
                }
                if (family == String.Empty)
                {
                    family = "لا يوجد ايصالات";
                }
                if (vecation == String.Empty)
                {
                    vecation = "لا يوجد ايصالات";
                }
                if (damaged == String.Empty)
                {
                    damaged = "لا يوجد ايصالات";
                }
                booksInfo += $"\n" +
            $"دفتر الايصالات رقم: {Rec.FirstReceipt} - {Rec.LastReceipt}\n" +
            $"عدد الايصالات العائلية: {Rec.FamilyReceiptNum}\n" +
            $"عدد الايصالات المركبة: {Rec.VecationReceiptNum}\n" +
            $"عدد الايصالات التالفة: {Rec.DamagedReceiptNum}\n" +
            $"قيمة دفتر الايصالات: {Rec.FamilyReceiptNum * Values.FamilyPrice + Rec.VecationReceiptNum * Values.VactionPrice}\n" +
            $"أرقام الايصالات العائلية المولدة من النظام:\n" +
            $"{family.Substring(0, family.Length - 2)}\n" +
            $"أرقام ايصالات المركبة المولدة من النظام:\n" +
            $"{vecation.Substring(0, vecation.Length - 2)}\n" +
            $"أرقام الايصالات التالفة المولدة من النظام:\n" +
            $"{damaged.Substring(0, damaged.Length - 2)}\n" +
            $"ملاحظات: {Rec.Notes}\n";
            }
            var cap = $"المحافظة: {book.Receipts.First().Region}\n" +
            $"اسم المركز: {book.Receipts.First().CenterName}\n" +
            $"رقم اشعار الحوالة: {book.RemittanceNum}\n" +
            $"{booksInfo}" +
            $"{erorrPrice}";
            await TelegramBotClient.SendTextMessageAsync(
            chatId: book.Id!,
            text: cap,
            cancellationToken: cancellationToken);
        }
        private async void UplodePhoto(Update update, CancellationToken cancellationToken)
        {
            if (SendReceipts.Any(a => a.Id == update.Message!.Chat.Id && a.Count == 5))
            {
                if (!Directory.Exists("photos")) { Directory.CreateDirectory("photos"); }
                var book = SendReceipts.Where(a => a.Id == update.Message!.Chat.Id).First();
                var fileId = update.Message!.Photo!.Last().FileId;
                var getFile = await TelegramBotClient.GetFileAsync(fileId);
                var filePath = getFile.FilePath;
                string destinationFilePath = $"photos/{book.RemittanceNum}.{filePath!.Split(".").Last()}";
                await using Stream fileStream = System.IO.File.Create(destinationFilePath);
                await TelegramBotClient.DownloadFileAsync(
                     filePath: filePath,
                     destination: fileStream,
                     cancellationToken: cancellationToken);
                book.ImageName = $"{book.RemittanceNum}.{filePath!.Split(".").Last()}";
                fileStream.Close();
                await ShowBook(book, cancellationToken);
                Email.SendMail(book);
                SendReceipts.Remove(book);
                await TelegramBotClient.SendTextMessageAsync(
                    chatId: update.Message.Chat.Id,
                    text: $"تمت العملية بنجاح\nشكرا جزيلاً...",
                    cancellationToken: cancellationToken);
            }
        }
        private void RegionOption(ChatId chatId)
        {
            List<string> list = new List<string>();
            foreach (var reg in Values.ListRegionCenter)
            {
                list.Add(reg.Region!);
            }
            AddChoiceButton(chatId, "الرجاء اختيار المحافظة", list);
        }
        private void CentersOption(ChatId chatId, string messageText)
        {
            var r = Values.ListRegionCenter;
            RegionCenters? RC = r.Where(a => a.Region == messageText).SingleOrDefault();
            if (RC != null)
            {
                AddChoiceButton(chatId, "الرجاء اختيار مركز البطاقة الذكية", RC.Centers!);
            }
        }
        private async Task<Receipt?> MessageToReceipt(ChatId chatId, string messageText, CancellationToken cancellationToken)
        {
            List<string> list = messageText.Split('\n').ToList();
            try
            {
                Receipt receipt = new Receipt
                {
                    CenterName = list.Where(a => a.Contains("اسم المركز: ")).FirstOrDefault()!.Split(": ").Last(),
                    Count = 0,
                    DamagedReceiptNum = int.Parse(list.Where(a => a.Contains("عدد الايصالات التالفة: ")).FirstOrDefault()!.Split(": ").Last()),
                    FamilyReceiptNum = int.Parse(list.Where(a => a.Contains("عدد الايصالات العائلية: ")).FirstOrDefault()!.Split(": ").Last()),
                    VecationReceiptNum = int.Parse(list.Where(a => a.Contains("عدد الايصالات المركبة: ")).FirstOrDefault()!.Split(": ").Last()),
                    Id = chatId,
                    Notes = list.Where(a => a.Contains("ملاحظات: ")).FirstOrDefault()!.Split(": ").Last(),
                    Region = list.Where(a => a.Contains("المحافظة: ")).FirstOrDefault()!.Split(": ").Last(),
                    FirstReceipt = int.Parse(list.Where(a => a.Contains("دفتر الايصالات رقم: ")).FirstOrDefault()!.Split(": ").Last().Split("-").First()),
                    LastReceipt = int.Parse(list.Where(a => a.Contains("دفتر الايصالات رقم: ")).FirstOrDefault()!.Split(": ").Last().Split("-").Last()),
                    DamagedReceiptList = new List<int>(),
                    FamilyReceiptList = new List<int>(),
                    VactionReceiptList = new List<int>()
                };
                if (!list[list.IndexOf("أرقام الايصالات العائلية المولدة من النظام:") + 1].Contains("لا يوجد"))
                { receipt.FamilyReceiptList = await ListReceiptNumber(chatId, list[list.IndexOf("أرقام الايصالات العائلية المولدة من النظام:") + 1], receipt.FamilyReceiptNum, cancellationToken); }
                if (!list[list.IndexOf("أرقام ايصالات المركبة المولدة من النظام:") + 1].Contains("لا يوجد"))
                { receipt.VactionReceiptList = await ListReceiptNumber(chatId, list[list.IndexOf("أرقام ايصالات المركبة المولدة من النظام:") + 1], receipt.VecationReceiptNum, cancellationToken); }
                if (!list[list.IndexOf("أرقام الايصالات التالفة:") + 1].Contains("لا يوجد"))
                { receipt.DamagedReceiptList = await ListReceiptNumber(chatId, list[list.IndexOf("أرقام الايصالات التالفة:") + 1], receipt.DamagedReceiptNum, cancellationToken); }
                if (receipt.FamilyReceiptList.Count != receipt.FamilyReceiptNum ||
                    receipt.VactionReceiptList.Count != receipt.VecationReceiptNum ||
                    receipt.DamagedReceiptList.Count != receipt.DamagedReceiptNum)
                {
                    return null;
                }
                return receipt;
            }
            catch
            {
                return null;
            }
        }
        private async void InserBook(ChatId chatId, SendReceipt sendReceipt, string messageText, CancellationToken cancellationToken)
        {
            var r = await MessageToReceipt(chatId, messageText, cancellationToken);
            bool b = true;
            if (r != null)
            {
                foreach (var item in sendReceipt.Receipts!)
                {
                    if (item.Region != r.Region || item.CenterName != r.CenterName)
                    {
                        b = false; break;
                    }
                }
                if (b)
                {
                    sendReceipt.Receipts!.Add(r);
                    if (sendReceipt.BookNumber != sendReceipt.Receipts!.Count)
                    {
                        await TelegramBotClient.SendTextMessageAsync(
                           chatId: chatId,
                           text: $"الرجاء ادخال معلومات دفتر الإيصالات رقم: {sendReceipt.Receipts.Count + 1}",
                           cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"الرجاء ادخال المبلغ المرسل بالحوالة",
                            cancellationToken: cancellationToken);
                        sendReceipt.Count++;
                    }
                }
                else
                {
                    await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"معلومات دفتر الايصالات لاتعود لنفس المركز",
                            cancellationToken: cancellationToken);
                }
            }
        }
        private async Task ShowReceipt(Receipt receipt, CancellationToken cancellationToken)
        {
            string booksInfo = "";
            string family = "";
            string vecation = "";
            string damaged = "";
            foreach (var item in receipt.FamilyReceiptList!)
            {
                family += $"{item} /";
            }
            foreach (var item in receipt.VactionReceiptList!)
            {
                vecation += $"{item} /";
            }
            foreach (var item in receipt.DamagedReceiptList!)
            {
                damaged += $"{item} /";
            }
            if (family == String.Empty)
            {
                family = "لا يوجد ايصالات  ";
            }
            if (vecation == String.Empty)
            {
                vecation = "لا يوجد ايصالات  ";
            }
            if (damaged == String.Empty)
            {
                damaged = "لا يوجد ايصالات  ";
            }
            booksInfo += $"\n" +
            $"دفتر الايصالات رقم: {receipt.FirstReceipt} - {receipt.LastReceipt}\n" +
            $"عدد الايصالات العائلية: {receipt.FamilyReceiptNum}\n" +
            $"عدد الايصالات المركبة: {receipt.VecationReceiptNum}\n" +
            $"عدد الايصالات التالفة: {receipt.DamagedReceiptNum}\n" +
            $"قيمة دفتر الايصالات: {receipt.FamilyReceiptNum * Values.FamilyPrice + receipt.VecationReceiptNum * Values.VactionPrice}\n" +
            $"أرقام الايصالات العائلية المولدة من النظام:\n" +
            $"{family.Substring(0, family.Length - 2)}\n" +
            $"أرقام ايصالات المركبة المولدة من النظام:\n" +
            $"{vecation.Substring(0, vecation.Length - 2)}\n" +
            $"أرقام الايصالات التالفة:\n" +
            $"{damaged.Substring(0, damaged.Length - 2)}\n" +
            $"ملاحظات: {receipt.Notes}\n";

            var cap = $"المحافظة: {receipt.Region}\n" +
            $"اسم المركز: {receipt.CenterName}\n" +
            $"{booksInfo}";
            await TelegramBotClient.SendTextMessageAsync(
            chatId: receipt.Id!,
            text: cap,
            cancellationToken: cancellationToken);
        }
        private async void SwitchReceipt(ChatId chatId, string messageText, Receipt receipt, CancellationToken cancellationToken)
        {
            switch (receipt.Count)
            {
                case 0:
                    {
                        receipt.Region = messageText;
                        CentersOption(chatId, messageText);
                        receipt.Count++;
                        break;
                    }
                case 1:
                    {
                        receipt.CenterName = messageText;
                        AddChoiceButton(chatId, "الرجاء ادخال رقم اول ايصال في دفتر الايصالات", new List<string>());
                        receipt.Count++;
                        break;
                    }
                case 2:
                    {
                        receipt.FirstReceipt = await IsNumber(chatId, messageText, cancellationToken);
                        if (receipt.FirstReceipt == null) { break; }
                        await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "الرجاء ادخال رقم اخر ايصال في دفتر الايصالات",
                            cancellationToken: cancellationToken);
                        receipt.Count++;
                        break;
                    }
                case 3:
                    {
                        receipt.LastReceipt = await IsNumber(chatId, messageText, cancellationToken);
                        if (receipt.LastReceipt == null) { break; }
                        if (receipt.LastReceipt - receipt.FirstReceipt == 49)
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "الرجاء ادخال عدد الايصالات العائلية",
                            cancellationToken: cancellationToken);
                            receipt.Count++;
                        }
                        else
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "رقم الايصال الأول أو رقم الايصال الأخير غير صحيح الرجاء اعادة ادخال الايصال الأول",
                            cancellationToken: cancellationToken);
                            receipt.Count--;
                        }
                        break;
                    }
                case 4:
                    {
                        var ch = await IsNumber(chatId, messageText, cancellationToken);
                        if (ch == null) { break; }
                        receipt.FamilyReceiptNum = ch ?? 0;
                        if (Receipt50(receipt))
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال عدد ايصالات المركبة",
                                cancellationToken: cancellationToken);
                            receipt.Count++;
                        }
                        else
                        {
                            receipt.FamilyReceiptNum = 0;
                            receipt.VecationReceiptNum = 0;
                            receipt.DamagedReceiptNum = 0;
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "عدد الايصالات اكبر من 50 وصل الرجاء اعادة ادخال عدد الايصالات",
                                cancellationToken: cancellationToken);
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال عدد الايصالات العائلية",
                                cancellationToken: cancellationToken);
                        }
                        break;
                    }
                case 5:
                    {
                        var ch = await IsNumber(chatId, messageText, cancellationToken);
                        if (ch == null) { break; }
                        receipt.VecationReceiptNum = ch ?? 0;
                        if (receipt.FirstReceipt == null) { break; }
                        if (Receipt50(receipt))
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال عدد الايصالات التالفة",
                                cancellationToken: cancellationToken);
                            receipt.Count++;
                        }
                        else
                        {
                            receipt.FamilyReceiptNum = 0;
                            receipt.VecationReceiptNum = 0;
                            receipt.DamagedReceiptNum = 0;
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "عدد الايصالات اكبر من 50 وصل الرجاء اعادة ادخال عدد الايصالات",
                                cancellationToken: cancellationToken);
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال عدد الايصالات العائلية",
                                cancellationToken: cancellationToken);
                            receipt.Count -= 1;
                        }
                        break;
                    }
                case 6:
                    {
                        var ch = await IsNumber(chatId, messageText, cancellationToken);
                        if (ch == null) { break; }
                        receipt.DamagedReceiptNum = ch ?? 0;
                        if (Receipt50(receipt))
                        {
                            if (receipt.FamilyReceiptNum != 0)
                            {
                                await TelegramBotClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "الرجاء ادخال الأرقام المولدة من بروفر للايصالات العائلية على الشكل: XXX,XXX,XXX,...",
                                    cancellationToken: cancellationToken);
                                receipt.Count++;
                            }
                            else if (receipt.VecationReceiptNum != 0)
                            {
                                await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال الأرقام المولدة من بروفر لايصالات المركبة على الشكل: XXX,XXX,XXX,...",
                                cancellationToken: cancellationToken);
                                receipt.Count += 2;
                            }
                            else if (receipt.DamagedReceiptNum != 0)
                            {
                                await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال الأرقام الايصالات التالفة على الشكل: XXX,XXX,XXX,...",
                                cancellationToken: cancellationToken);
                                receipt.Count += 3;
                            }

                        }
                        else
                        {
                            receipt.FamilyReceiptNum = 0;
                            receipt.VecationReceiptNum = 0;
                            receipt.DamagedReceiptNum = 0;
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "عدد الايصالات اكبر من 50 وصل الرجاء اعادة ادخال عدد الايصالات",
                                cancellationToken: cancellationToken);
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال عدد الايصالات العائلية",
                                cancellationToken: cancellationToken);
                            receipt.Count -= 2;
                        }
                        break;
                    }
                case 7:
                    {
                        receipt.FamilyReceiptList = await ListReceiptNumber(chatId, messageText, receipt.FamilyReceiptNum, cancellationToken);
                        if (receipt.FamilyReceiptNum == receipt.FamilyReceiptList.Count)
                        {
                            if (receipt.VecationReceiptNum != 0)
                            {
                                await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال الأرقام المولدة من بروفر لايصالات المركبة على الشكل: XXX,XXX,XXX,...",
                                cancellationToken: cancellationToken);
                                receipt.Count++;
                            }
                            else if (receipt.DamagedReceiptNum != 0)
                            {
                                await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال الأرقام الابصالات التالفة على الشكل: XXX,XXX,XXX,...",
                                cancellationToken: cancellationToken);
                                receipt.Count += 2;
                            }
                        }
                        break;
                    }
                case 8:
                    {
                        receipt.VactionReceiptList = await ListReceiptNumber(chatId, messageText, receipt.VecationReceiptNum, cancellationToken);
                        if (receipt.VecationReceiptNum == receipt.VactionReceiptList.Count)
                        {
                            if (receipt.DamagedReceiptNum != 0)
                            {
                                await TelegramBotClient.SendTextMessageAsync(
                                    chatId: chatId,
                                    text: "الرجاء ادخال الأرقام الابصالات التالفة على الشكل: XXX,XXX,XXX,...",
                                    cancellationToken: cancellationToken);
                                receipt.Count++;
                            }
                            else
                            {
                                await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء كتاب الملاحظات ان وجدت او كتابة لا يوجد",
                                cancellationToken: cancellationToken);
                                receipt.Count += 2;
                            }
                        }
                        break;
                    }
                case 9:
                    {
                        receipt.DamagedReceiptList = await ListReceiptNumber(chatId, messageText, receipt.DamagedReceiptNum, cancellationToken);
                        if (receipt.DamagedReceiptNum == receipt.DamagedReceiptList.Count)
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء كتاب الملاحظات ان وجدت او كتابة لا يوجد",
                                cancellationToken: cancellationToken);
                            receipt.Count++;
                        }
                        break;
                    }
                case 10:
                    {
                        receipt.Notes = messageText;
                        await ShowReceipt(receipt, cancellationToken);
                        Receipts.Remove(receipt);
                        break;
                    }
            }
        }
        private async void SwitchSendReceipt(ChatId chatId, string messageText, SendReceipt receipt, CancellationToken cancellationToken)
        {
            switch (receipt.Count)
            {
                case 0:
                    {
                        if (await IsRemittanceNumber(chatId, messageText, cancellationToken))
                        {
                            receipt.RemittanceNum = messageText;
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "الرجاء ادخال عدد دفاتر الايصالات المرسلة",
                                cancellationToken: cancellationToken);
                            receipt.Count++;
                        }
                        break;
                    }
                case 1:
                    {
                        receipt.BookNumber = await IsNumber(chatId, messageText, cancellationToken);
                        if (receipt.BookNumber == null) { break; }
                        await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "الرجاء ادخال معلومات الدفتر رقم : 1",
                            cancellationToken: cancellationToken);
                        receipt.Count++;
                        break;
                    }
                case 2:
                    {
                        InserBook(chatId, receipt, messageText, cancellationToken);
                        break;
                    }
                case 3:
                    {
                        receipt.Price = await IsNumber(chatId, messageText, cancellationToken);
                        if (receipt.Price == null) { break; }
                        int p = 0;
                        foreach (var item in receipt.Receipts!)
                        {
                            p += item.FamilyReceiptNum * Values.FamilyPrice;
                            p += item.VecationReceiptNum * Values.VactionPrice;
                        }
                        if (receipt.Price == p)
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "الرجاء ارسال صورة واضحة لإشعار الحوالة ",
                            cancellationToken: cancellationToken);
                            receipt.Count += 2;
                        }
                        else
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"الرجاء ادخال سبب وجود خطأ بقيمة {receipt.Price - p} في مبلغ الحوالة",
                            cancellationToken: cancellationToken);
                            receipt.Count++;
                        }
                        break;
                    }
                case 4:
                    {
                        receipt.ErorrPriceNote = messageText;
                        await TelegramBotClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "الرجاء ارسال صورة واضحة لإشعار الحوالة ",
                            cancellationToken: cancellationToken);
                        receipt.Count++;
                        break;
                    }
            }
        }
        private async void SwitchAddUser(ChatId chatId, string messageText, UserBot user, CancellationToken cancellationToken)
        {
            if (user.Region == null)
            {
                user.Region = messageText;
                CentersOption(chatId, messageText);
            }
            else if (user.Center == null)
            {
                user.Center = messageText;
                AddChoiceButton(chatId, "الرجاء ادخال المعرف الخاص بتلغرام لمشرف المركز", new() { });

            }
            else if (user.Id == null)
            {
                user.Id = long.Parse(messageText.ToString());
                var b = await Admin.AddUserToBot(long.Parse(chatId.ToString()), user);
                if (b)
                {
                    await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "تمت اضافة المشرف بنجاح",
                                cancellationToken: cancellationToken);
                }
                else
                {
                    await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "هناك خطأ ما الرجاء الاتصال بالمسؤول",
                                cancellationToken: cancellationToken);
                }
            }
        }
        private async Task ProgressText(string who, Message message, CancellationToken cancellationToken)
        {
            if (message.Text is not { } messageText)
                return;
            var chatId = message.Chat.Id;
            var chatname = message.Chat.Username;
            if (Values.Option!.Any(a => a == messageText) || Values.OptionAdmin!.Any(a => a == messageText) || Values.OptionSuperAdmin!.Any(a => a == messageText) || messageText == "العودة إلى البداية" || messageText == "/start")
            {

                if (SendReceipts.Any(a => a.Id == chatId))
                {
                    var r = SendReceipts.Where(a => a.Id == chatId).FirstOrDefault();
                    SendReceipts.Remove(r!);
                }
                if (Receipts.Any(a => a.Id == chatId))
                {
                    var r = Receipts.Where(a => a.Id == chatId).FirstOrDefault();
                    Receipts.Remove(r!);
                }
                if (UserBots.Any(a => a.adminAdd == chatId))
                {
                    var r = UserBots.Where(a => a.adminAdd == chatId).FirstOrDefault();
                    UserBots.Remove(r!);
                }
                AddRegion = false;
                switch (messageText)
                {
                    case "إضافة حوالة مالية":
                        {
                            Console.WriteLine(who);
                            SendReceipt sendReceipt = new()
                            {
                                Id = chatId,
                                Count = 0,
                                Receipts = new List<Receipt>(),
                            };
                            SendReceipts.Add(sendReceipt);
                            AddChoiceButton(chatId, "الرجاء ادخال رقم الحوالة المالية", new List<string>());
                            break;
                        }
                    case "اضافة معلومات دفتر ايصالات":
                        {
                            Console.WriteLine(who);
                            Receipt receipt = new()
                            {
                                Id = chatId,
                                Count = 0,
                                DamagedReceiptNum = 0,
                                FamilyReceiptNum = 0,
                                VecationReceiptNum = 0,
                                DamagedReceiptList = new(),
                                FamilyReceiptList = new(),
                                VactionReceiptList = new(),
                            };
                            Receipts.Add(receipt);
                            RegionOption(chatId);
                            break;
                        }
                    case "إضافة مستخدم":
                        {
                            UserBot userBot = new() { adminAdd = chatId };
                            UserBots.Add(userBot);
                            RegionOption(chatId);
                            break;
                        }
                    case "إضافة محافظة":
                        {
                            AddRegion = true;
                            AddChoiceButton(chatId, "الرجاء ادخال معلومات المحافظة بالصيغة التالية بدون فواصل\nاسم المحافظة-البريد الإلكتروني لمشرف المحافظة", new() { });
                            break;
                        }
                    case "حذف محافظة":
                        {
                            RemoveRegion = true;
                            RegionOption(chatId);
                            break;
                        }
                    case "إضافة مشرف قناة":
                        {
                            AddSuperAdmin = true;
                            AddChoiceButton(chatId, "الرجاء ادخال معلومات المشرف بدون فواصل على الشكل\nمعرف التلغرام:اسم المشرف", new() { });
                            break;
                        }
                    case "حذف مشرف قناة":
                        {
                            DeleteSuperAdmin = true;
                            var s = Admin.GetSuperAdminList(chatId);
                            AddChoiceButton(chatId, $"{s}الرجاء كتابة معرف المشرف المراد حذفه", new() { });
                            break;
                        }
                    case "إضافة مركز":
                        {
                            AddCenter = true;
                            AddChoiceButton(chatId, "الرجاء ادخال اسم المركز", new() { });
                            break;
                        }
                    case "حذف مركز":
                        {
                            DeleteCenter = true;
                            var r = Admin.GetAdminRegion(chatId);
                            if (r != null)
                            {
                                CentersOption(chatId, r);
                            }
                            break;
                        }
                    case "إضافة مشرف محافظة":
                        {
                            AddAdmin = new() { Id = null, Region = null };
                            RegionOption(chatId);
                            break;
                        }
                    case "حذف مشرف محافظة":
                        {
                            DeleteAdmin = true;
                            var r = Admin.GetAdmin(chatId);
                            AddChoiceButton(chatId, $"{r}\nالرجاء ادخال معرف مشرف المحافظة", new());
                            break;
                        }
                    default:
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                                chatId: chatId,
                                text: "أهلا بك في بوت مراكز البطاقة الذكية للحوالات المالية",
                                cancellationToken: cancellationToken);
                            List<string> option = new() { };
                            option.AddRange(Values.Option);
                            if (Admin.CheckAdmin(message.Chat.Id!))
                            {
                                option.AddRange(Values.OptionAdmin);
                            }
                            if (Admin.CheckSuperAdmin(message.Chat.Id!))
                            {
                                option.AddRange(Values.OptionAdmin);
                                option.AddRange(Values.OptionSuperAdmin);
                            }
                            AddChoiceButton(chatId, "إختر خيار من القائمة", option);
                            break;
                        }
                }
            }
            else
            {
                if (Receipts.Any(a => a.Id == chatId))
                {
                    Receipt receipt = Receipts.Where(a => a.Id == chatId).SingleOrDefault()!;
                    SwitchReceipt(chatId, messageText, receipt, cancellationToken);
                }
                else if (SendReceipts.Any(a => a.Id == chatId))
                {
                    SendReceipt receipt = SendReceipts.Where(a => a.Id == chatId).SingleOrDefault()!;
                    SwitchSendReceipt(chatId, messageText, receipt, cancellationToken);
                }
                else if (UserBots.Any(a => a.adminAdd == chatId))
                {
                    UserBot user = UserBots.Where(a => a.adminAdd == chatId).SingleOrDefault()!;
                    SwitchAddUser(chatId, messageText, user, cancellationToken);
                }
                else if (AddRegion)
                {
                    var list = messageText.Split("-").ToList();
                    if (list.Count == 2)
                    {
                        var b = await Admin.AddRegion(chatId, list[0], list[1]);
                        if (b)
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "تمت اضافة المحافظة بنجاح",
                                        cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await TelegramBotClient.SendTextMessageAsync(
                                        chatId: chatId,
                                        text: "هناك خطأ ما الرجاء الاتصال بالمسؤول",
                                        cancellationToken: cancellationToken);
                        }
                    }
                    AddRegion = false;
                }
                else if (RemoveRegion)
                {
                    var b = await Admin.RemovRegion(chatId, messageText);
                    if (b)
                    {
                        AddChoiceButton(chatId, "تمت حذف المحافظة بنجاح", new() { });
                    }
                    else
                    {
                        AddChoiceButton(chatId, "هناك خطأ ما الرجاء الاتصال بالمسؤول", new() { });
                    }
                    RemoveRegion = false;
                }
                else if (AddSuperAdmin)
                {
                    var s = messageText.Split(":").ToList();
                    if (s.Count == 2)
                    {
                        var b = await Admin.AddSuperAdmin(chatId, new() { Id = long.Parse(s[0]), Name = s[1] });
                        if (b)
                        {
                            AddChoiceButton(chatId, "تمت إضافة المشرف بنجاح", new() { });
                        }
                        else
                        {
                            AddChoiceButton(chatId, "هناك خطأ ما الرجاء الاتصال بالمسؤول", new() { });
                        }
                    }
                    AddSuperAdmin = false;
                }
                else if (DeleteSuperAdmin)
                {
                    var b = await Admin.RemoveSuperAdmin(chatId, long.Parse(messageText));
                    if (b)
                    {
                        AddChoiceButton(chatId, "تمت حذف المشرف بنجاح", new() { });
                    }
                    else
                    {
                        AddChoiceButton(chatId, "هناك خطأ ما الرجاء الاتصال بالمسؤول", new() { });
                    }
                    DeleteSuperAdmin = false;
                }
                else if (AddCenter)
                {
                    var b = await Admin.AddCenter(chatId, messageText);
                    if (b)
                    {
                        AddChoiceButton(chatId, "تم إضافة المركز بنجاح", new() { });
                    }
                    else
                    {
                        AddChoiceButton(chatId, "هناك خطأ ما الرجاء الاتصال بالمسؤول", new() { });
                    }
                    AddCenter = false;
                }
                else if (DeleteCenter)
                {
                    var b = await Admin.DeleteCenter(chatId, messageText);
                    if (b)
                    {
                        AddChoiceButton(chatId, "تم حذف المركز بنجاح", new() { });
                    }
                    else
                    {
                        AddChoiceButton(chatId, "هناك خطأ ما الرجاء الاتصال بالمسؤول", new() { });
                    }
                    DeleteCenter = false;
                }
                else if (AddAdmin != null)
                {
                    if (AddAdmin.Region == null)
                    {
                        AddAdmin.Region = messageText;
                        AddChoiceButton(chatId, "الرجاء ادخال معرف التلغرام الخاص بالمشرف", new());
                    }
                    else
                    {
                        AddAdmin.Id = long.Parse(messageText);                        
                        var b = await Admin.AddAdminToBot(AddAdmin);
                        if (b)
                        {
                            AddChoiceButton(chatId, "تم اضافة المشرف بنجاح", new() { });
                        }
                        else
                        {
                            AddChoiceButton(chatId, "هناك خطأ ما الرجاء الاتصال بالمسؤول", new() { });
                        }
                        AddAdmin = null;
                    }
                }
                else if (DeleteAdmin)
                {
                    var b = await Admin.DeleteAdminBot(long.Parse(messageText));
                }
            }
        }
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            if (Admin.CheckUser(message.Chat.Id!) ||  Admin.CheckAdmin(message.Chat.Id!) || Admin.CheckSuperAdmin(message.Chat.Id!))
            {
                if (update.Message!.Photo != null)
                {
                    UplodePhoto(update, cancellationToken);
                    Console.WriteLine("Uplode Photo");
                }
                else
                {
                    await ProgressText($"{message.Chat.Id}:{message.Chat.Username}", message, cancellationToken);
                }
                Console.WriteLine(message.Text);
            }
        }
    }
}
