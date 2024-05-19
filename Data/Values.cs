using CentersTakamol.Controller;
using CentersTakamol.Models;
using MimeKit;

namespace CentersTakamol.Data
{
    internal class Values
    {
        public static int FamilyPrice = 10000;
        public static int VactionPrice = 5500;
        public static string TelegramBotId = "7191100562:AAFnhjnbD6I8EzPJWI9kFyLkDq19n-Z97So";
        public static List<string> Option = new List<string> {
            "اضافة معلومات دفتر ايصالات","إضافة حوالة مالية"
        };
        public static List<string> OptionAdmin = new List<string> {
            "إضافة مستخدم","إضافة مركز","حذف مركز"
        };
        public static List<string> OptionSuperAdmin = new List<string> {
            "إضافة محافظة","حذف محافظة","إضافة مشرف محافظة","حذف مشرف محافظة","إضافة مشرف قناة","حذف مشرف قناة"
        };
        public static List<RegionSupervisor> ListRegionSupervisor = Admin.GetRegionSupervisors(); 
        public static List<RegionCenters> ListRegionCenter = Admin.GetRegionCenter();

    }
}
