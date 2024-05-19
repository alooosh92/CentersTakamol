using CentersTakamol.Data;
using CentersTakamol.Models;
using MimeKit;
using System.Text.Json;

namespace CentersTakamol.Controller
{
    internal class Admin
    {        
        public static async Task<bool> AddUserToBot(long userId,UserBot user)
        {
            if(user == null || user.Id == null || user.Center == null || user.Region == null) { return false; }
            string jsonString = JsonSerializer.Serialize(user, typeof(UserBot), new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });            
            var admin = File.ReadLines("AdminBot.json").Where(a=>a.Contains(userId.ToString())&&a.Contains(user.Region)).ToList();
            if(admin.Count == 0) { return false; }
            var readFile =  File.ReadLines("UserBot.json").Where(a => !(a.Contains(user.Region) && a.Contains(user.Center))).ToList();
            readFile.Add($"{jsonString}");
            await File.WriteAllLinesAsync("UserBot.json", readFile);
            return true;
        }
        public static async Task<bool> AddAdminToBot(AdminBot user)
        {
            if (user == null || user.Id == null || user.Region == null) { return false; }
            string jsonString = JsonSerializer.Serialize(user, typeof(AdminBot), new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });         
            var readFile = File.ReadLines("AdminBot.json").Where(a => !a.Contains(user.Region)).ToList();
            readFile.Add($"{jsonString}");
            await File.WriteAllLinesAsync("AdminBot.json", readFile);
            return true;
        }
        public static async Task<bool> DeleteAdminBot(long id)
        {
            var admin = File.ReadLines("SuperAdminBot.json").Where(a => a.Contains(id.ToString())).ToList();
            if (admin.Count == 0) { return false; }
            var readFile = File.ReadLines("AdminBot.json").Where(a => !a.Contains(id.ToString())).ToList();
            await File.WriteAllLinesAsync("AdminBot.json", readFile);
            Values.ListRegionSupervisor = GetRegionSupervisors();
            Values.ListRegionCenter = GetRegionCenter();
            return true;
        }
        public static string? GetAdmin(long userId)
        {
            var readFile= File.ReadLines("SuperAdminBot.json").Where(a => a.Contains(userId.ToString())).ToList();
            if (readFile.Count == 0) { return null; }
            var j = JsonSerializer.Deserialize<AdminBot>(readFile.First());
            return j!.Region;
        }
        public static bool CheckAdmin(long userId)
        {
            var readFile = File.ReadLines("AdminBot.json").Where(a => a.Contains(userId.ToString())).ToList();
            var readFile1 = File.ReadLines("SuperAdminBot.json").Where(a => a.Contains(userId.ToString())).ToList();
            if (readFile.Count == 0 && readFile1.Count == 0) { return false; }
            return true;
        }
        public static bool CheckSuperAdmin(long userId)
        {
            var readFile = File.ReadLines("SuperAdminBot.json").Where(a => a.Contains(userId.ToString())).ToList();
            if (readFile.Count == 0) { return false; }
            return true;
        }
        public static bool CheckUser(long userId)
        {
            var readFile = File.ReadLines("UserBot.json").Where(a => a.Contains(userId.ToString())).ToList();
            if(readFile.Count == 0) { return false; }
            return true;
        }
        public static string? GetSuperAdminList(long userId)
        {
            if (!CheckSuperAdmin(userId)) { return null; }
            string list = "";
            var readFile = File.ReadLines("SuperAdminBot.json").ToList();
            foreach (var line in readFile)
            {
                var j = JsonSerializer.Deserialize<AddSuperAdmin>(line);
                if(j != null)
                {
                    list += ($"{j.Id}: {j.Name}\n");
                }               
            }
            return list;
        }
        public static async Task<bool> RemovRegion(long userId, string regionName)
        {
            var admin = File.ReadLines("SuperAdminBot.json").Where(a => a.Contains(userId.ToString())).ToList();
            if (admin.Count == 0) { return false; }
            var readFile = File.ReadLines("RegionBot.json").Where(a => !a.Contains(regionName)).ToList();
            await File.WriteAllLinesAsync("RegionBot.json", readFile);
            Values.ListRegionSupervisor = GetRegionSupervisors();
            Values.ListRegionCenter = GetRegionCenter();
            return true;
        }
        public static async Task<bool> AddRegion(long userId,string regionName,string email)
        {
            AddRegion regionSupervisor = new AddRegion()
            {
                Region = regionName,
                Name = email.Split("@").First(),
                Email = email
            };
            var admin = File.ReadLines("SuperAdminBot.json").Where(a => a.Contains(userId.ToString())).ToList();
            if (admin.Count == 0) { return false; }            
            string jsonString = JsonSerializer.Serialize(regionSupervisor,typeof(AddRegion), new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });            
            var readFile = File.ReadLines("RegionBot.json").Where(a => !a.Contains(regionName)).ToList();
            readFile.Add($"{jsonString}");
            await File.WriteAllLinesAsync("RegionBot.json", readFile);
            Values.ListRegionSupervisor = GetRegionSupervisors();
            Values.ListRegionCenter = GetRegionCenter();
            return true;
        }
        public static bool ConfremAuth(long userId,string region,string center)
        {
            var readFile = File.ReadLines("UserBot.json").Where(a => a.Contains(userId.ToString()) && a.Contains(region) && a.Contains(center)).ToList();
            if (readFile.Count == 0) { return false; }
            return true;
        }
        public static async Task<bool> RemoveSuperAdmin(long userId,long userDeleteId)
        {
            if(!CheckSuperAdmin(userId)) { return false; }
            var readFile = File.ReadLines("SuperAdminBot.json").Where(a => !a.Contains(userDeleteId.ToString())).ToList();
            await File.WriteAllLinesAsync("SuperAdminBot.json", readFile);
            return true;
        }
        public static async Task<bool> AddSuperAdmin(long userId, AddSuperAdmin userDeleteId)
        {
            if (!CheckSuperAdmin(userId)) { return false; }
            var readFile = File.ReadLines("SuperAdminBot.json").ToList();
            readFile.Add(JsonSerializer.Serialize(userDeleteId,typeof(AddSuperAdmin), new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) }));
            await File.WriteAllLinesAsync("SuperAdminBot.json", readFile);
            return true;
        }
        public static string? GetAdminRegion(long userId)
        {
            var readFile = File.ReadLines("AdminBot.json").Where(a=>a.Contains(userId.ToString())).ToList();
            var readFile1 = File.ReadLines("SuperAdminBot.json").Where(a => a.Contains(userId.ToString())).ToList();
            if (readFile.Count == 0 && readFile1.Count == 0) { return null; }
            var j = JsonSerializer.Deserialize<AdminBot>(readFile.First());
            return j!.Region;
        }
        public static async Task<bool> AddCenter(long userId,string center)
        {
            if(!CheckAdmin(userId)) { return false; }
            var readFile = File.ReadLines("RegionCenter.json").ToList();
            var c = new AddCenter()
            {
                Region = GetAdminRegion(userId),
                Name = center,
            };
            readFile.Add(JsonSerializer.Serialize(c, typeof(AddCenter), new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) }));
            await File.WriteAllLinesAsync("RegionCenter.json", readFile);
            Values.ListRegionSupervisor = GetRegionSupervisors();
            Values.ListRegionCenter = GetRegionCenter();
            return true;
        }
        public static async Task<bool> DeleteCenter(long userId, string center)
        {
            if (!CheckAdmin(userId)) { return false; }            
            var readFile = File.ReadLines("RegionCenter.json").Where(a=>!(a.Contains(center)&& a.Contains(GetAdminRegion(userId)!))).ToList();
            await File.WriteAllLinesAsync("RegionCenter.json", readFile);
            Values.ListRegionSupervisor = GetRegionSupervisors();
            Values.ListRegionCenter = GetRegionCenter();
            return true;
        }
        public static List<RegionCenters> GetRegionCenter()
        {
            List<RegionCenters> list = new();
            var center = File.ReadLines("RegionCenter.json").ToList();
            var region = File.ReadLines("RegionBot.json").ToList();
            foreach (var reg in region)
            {
                var r = JsonSerializer.Deserialize<AddRegion>(reg);
                list.Add(new() { Region = r!.Region, Centers = new() { } });
            }
            foreach (var cen in center)
            {
                var rc = JsonSerializer.Deserialize<AddCenter>(cen);
                if (rc != null && list.Any(a => a.Region == rc.Region))
                {
                    if (list.Any(a => a.Region == rc!.Region))
                    {
                        list.Where(a => a.Region == rc!.Region).SingleOrDefault()!.Centers!.Add(rc!.Name!);
                    }
                }
            }
            return list;
        }
        public static List<RegionSupervisor> GetRegionSupervisors()
        {
            var readFile = File.ReadLines("RegionBot.json").ToList();
            List<RegionSupervisor> list = new();
            foreach (var super in readFile)
            {
                var rc = JsonSerializer.Deserialize<AddRegion>(super);
                if (rc != null)
                {
                    list.Add(new() { Region = rc.Region, Supervisor = new MailboxAddress(rc.Email!.Split("@").First(), rc.Email) });
                }
            }
            return list;
        } 
    }
}
