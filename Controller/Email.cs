using CentersTakamol.Data;
using CentersTakamol.Models;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;

namespace CentersTakamol.Controller
{
    internal class Email
    {
        public static void SendMail(SendReceipt book)
        {
            var imapServer = "mail.takamol.me";
            var imapPort = 993;
            var smtpServer = "mail.takamol.me";
            var smtpPort = 465;
            //
            var username = "alaa.baaj@takamol.me";
            var password = "bj@23T@k";
            var message = new MimeMessage();
            //
            var supervisor = Values.ListRegionSupervisor;
            message.From.Add(new MailboxAddress("Alaa Baaj", "alaa.baaj@takamol.me"));
            message.To.Add(new MailboxAddress("Bothaina Aljaramani", "bothaina.aljaramani@takamol.me"));
            message.Cc.Add(new MailboxAddress("MHD Khaldoun Adde", "mhd.khaldoun.adde@takamol.me"));
            message.Cc.Add(new MailboxAddress("Sarah Naim", "sarah.naim@takamol.me"));
            message.Cc.Add(new MailboxAddress("Khaled Sahli", "khaled.sahli@takamol.me"));
            message.Cc.Add(new MailboxAddress("Financial Controller", "financial.controller@takamol.me"));
            message.Cc.Add(new MailboxAddress("Alaa Khalil", "alaa.khalil@takamol.me"));
            message.Cc.Add(new MailboxAddress("Joul Alyan", "joul.alyan@takamol.me"));
            message.Cc.Add(new MailboxAddress("Safwan Azhari", "safwan.azhari@takamol.me"));
            message.Cc.Add(new MailboxAddress("Mouhammad Fadel", "mouhammad.fadel@takamol.me"));
            message.Cc.Add(new MailboxAddress("Salam Aldeghli", "salam.aldeghli@takamol.me"));
            message.Cc.Add(new MailboxAddress("Zubaida Fakhany", "zubaida.fakhany@takamol.me"));
            message.Cc.Add(supervisor.Where(a=>a.Region == book.Receipts!.First().Region).First().Supervisor);            
            message.Subject = $"تفاصيل دفاتر ايصالات - {book.Receipts!.First().Region}";
            var attachment = new MimePart("Image", book.ImageName!.Split(".").Last())
            {
                Content = new MimeContent(File.OpenRead($"photos/{book.ImageName}")),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = book.ImageName
            };

            var multipart = new Multipart("mixed")
            {
                attachment,
                new TextPart("html")
                {
                    Text = BodyEmail(book),
                },
            };
            message.Body = multipart;
            using (var imapClient = new ImapClient())
            {
                imapClient.Connect(imapServer, imapPort, true);
                imapClient.Authenticate(username, password);

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(smtpServer, smtpPort, true);
                    smtpClient.Authenticate(username, password);
                    smtpClient.Send(message);
                    smtpClient.Disconnect(true);
                }
                imapClient.Disconnect(true);
            }
        }
        private static string Tabel(SendReceipt multiReceiptModel, Receipt receipt)
        {
            return $@"
<br>
    <table border=""1"">
        <tr> 
            <td colspan=""4"" class=""tdTotal""><p class=""textBlack"">محافظة {multiReceiptModel.Receipts!.First().Region}</p></td>            
        </tr>
        <tr>
            <td colspan=""2"" class=""tdDarkGray""><p class=""textWhite"">اسم المركز</p></td>
            <td colspan=""2"" class=""tdWhite""><p class=""textBlack"">{multiReceiptModel.Receipts!.First().CenterName}</p></td>            
        </tr>
        <tr>
            <td colspan=""2"" class=""tdDarkGray""><p class=""textWhite multeLine"">رقم اشعار حوالة الهرم</p></td>
            <td colspan=""2"" class=""tdWhite""><p class=""textBlack"">{multiReceiptModel.RemittanceNum}</p></td>            
        </tr>
        <tr>
            <td colspan=""2"" class=""tdDarkGray""><p class=""textWhite"">نوع النظام</p></td>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack"">جديد</p></td>            
        </tr>
        <tr>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack"">نطاق الايصالات</p></td>
            <td colspan=""2"" class=""tdWhite""><p class=""textBlack"">{receipt.FirstReceipt} - {receipt.LastReceipt}</p></td>             
        </tr>
        <tr>
            <td class=""tdDarkGray""><p class=""textWhite multeLine"">نوع الوصل</p></td>
            <td class=""tdDarkGray""><p class=""textWhite multeLine"">قيمة الوصل</p></td>
            <td class=""tdDarkGray""><p class=""textWhite"">عدد الايصالات</p></td>
            <td class=""tdDarkGray""><p class=""textWhite"">الإجمالي</p></td>         
        </tr>
        <tr>
            <td class=""tdLaityGray""><p class=""textBlack"">عائلي</p></td>
            <td class=""tdLaityGray""><p class=""textBlack"">{Values.FamilyPrice}</p></td> 
            <td class=""tdWhite""><p class=""textBlack"">{receipt.FamilyReceiptNum}</p></td> 
            <td class=""tdWhite""><p class=""textBlack"">{receipt.FamilyReceiptNum * Values.FamilyPrice}</p></td>            
        </tr>
        <tr>
            <td class=""tdLaityGray""><p class=""textBlack"">مركبة</p></td>
            <td class=""tdLaityGray""><p class=""textBlack"">{Values.VactionPrice}</p></td>
            <td class=""tdWhite""><p class=""textBlack"">{receipt.VecationReceiptNum}</p></td> 
            <td class=""tdWhite""><p class=""textBlack"">{receipt.VecationReceiptNum * Values.VactionPrice}</p></td>            
        </tr>
        <tr>
            <td class=""tdLaityGray""><p class=""textBlack"">مركبة</p></td>
            <td class=""tdLaityGray""><p class=""textBlack"">{Values.VactionPrice}</p></td>
            <td class=""tdWhite""><p class=""textBlack"">{receipt.DamagedReceiptNum}</p></td> 
            <td class=""tdWhite""><p class=""textBlack"">0</p></td>            
        </tr>
        <tr>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack"">اجمالي الكل</p></td>
            <td colspan=""2"" class=""tdTotal""><p class=""textBlack"">{receipt.FamilyReceiptNum * Values.FamilyPrice + receipt.VecationReceiptNum * Values.VactionPrice}</p></td>            
        </tr>
        <tr>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack,pMulteLine"">ايصالات عائلية</p></td>
            <td colspan=""2"" class=""tdWhite""><p class=""textBlack,pMulteLine"">{ReceiptToStrin(receipt.FamilyReceiptList!)}</p></td>            
        </tr>
        <tr>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack,pMulteLine"">ايصالات مركبة</p></td>
            <td colspan=""2"" class=""tdWhite""><p class=""textBlack,pMulteLine"">{ReceiptToStrin(receipt.VactionReceiptList!)}</p></td>            
        </tr>
         <tr>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack,pMulteLine"">ايصالات ملغاة</p></td>
            <td colspan=""2"" class=""tdWhite""><p class=""textBlack,pMulteLine"">{ReceiptToStrin(receipt.DamagedReceiptList!)}</p></td>            
        </tr>
        <tr>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack,pMulteLine"">ملاحظات</p></td>
            <td colspan=""2"" class=""tdWhite""><p class=""textBlack,pMulteLine"">{receipt.Notes}</p></td>            
        </tr>
    </table>
<br>
";
        }
        private static string BodyEmail(SendReceipt book)
        {
            string erorrPrice = "";
            if(book.ErorrPriceNote != null && book.ErorrPriceNote != "") 
            {
                int p = 0;
                foreach (var item in book.Receipts!)
                {
                    p += item.FamilyReceiptNum * Values.FamilyPrice;
                    p += item.VecationReceiptNum * Values.VactionPrice;
                }
                erorrPrice = $"<h2 class=\"textRed\">ملاحظة: يوجد فرق بقيمة {book.Price - p} بالحوالة المرسلة</h2>" +
                    $"<h2 class=\"textRed\">{book.ErorrPriceNote}</h2>";
            }
            string tabels = "";
            foreach (var rec in book.Receipts!)
            {
                tabels += Tabel(book, rec);
            }
            return @$"
<!DOCTYPE html>
<html lang=""ar"">
<head>
    <meta charset='utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <title>Page Title</title>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <style>
        p{{
            padding: 5px;
            margin: 0px;
            font-weight: 700;
            font-size: 16;
            font-family: ""Calibri"";
            text-align: center;           
        }} 
        b{{
            font-size: ""16"";
            text-align: center;
        }} 
        div{{
            direction: ltr;    
                      
        }}
        .b1{{
            font-family: ""Freestyle Script"";  
            padding-left: 21px;
        }} 
        .b2{{
            font-family: ""Vladimir Script"";
        }}
        td{{
            text-align: center;
        }}
        .multeLine{{             
            white-space: nowrap;
        }}
        .tdDarkGray{{           
            background-color: #808080; 
        }}
        .tdLaityGray{{
            background-color: #d9d9d9; 
        }}
        .tdWhite{{
            background-color: #FFFFFF; 
        }}
        .tdTotal{{
            background-color: #f4b084; 
        }}
        .textWhite{{
            color: #FFFFFF;
        }}
        .textBlack{{
            color: #000000;
        }}
        .textRed{{
            color: #FF0000;
        }}
    </style>
</head>
<body dir=""rtl"">
    <h2>السادة المحترمين</h2>
    <h3>في ما يلي تفصيل دفتر ايصالات</h3>
    <h3>علماً انه تم تحويل قيمته بموجب الإشعار المرفق </h3>
    {erorrPrice}
    {tabels}
    <h3>للاطلاع وإجراء اللازم</h3>
    <h3>ولكم جزيل الشكر...</h3>
    <div>
        <b class=""b1"">Aleppo: Alaa MHD Nidal Baaj</b><br>
        <b class=""b2"">0956108642 - 0965771204</b>  
    </div>  
</body>
</html>
";
        }
        private static string ReceiptToStrin(List<int> listReceipt)
        {
            string val = "/";
            int i = 0;
            foreach (var item in listReceipt)
            {
                val += $" {item} /";
                i++;
                if (i == 10)
                {
                    i = 0;
                    val += "\n";
                }
            }
            return val;
        }
        private static string DemegReceipt(List<int> listReceipt)
        {
            string val = @"
        <tr>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack"">لا يوجد</p></td>            
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack"">لا يوجد</p></td>            
        </tr>
";
            if (listReceipt.Count == 0) { return val; }
            foreach (var item in listReceipt)
            {
                val += @$"
        <tr>
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack"">{item}</p></td>            
            <td colspan=""2"" class=""tdLaityGray""><p class=""textBlack"">تالف</p></td>            
        </tr>
";
            }
            return "";
        }
    }
}
