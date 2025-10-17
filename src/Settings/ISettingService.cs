using AasDemoapp.Database.Model;

namespace AasDemoapp.Settings
{
    public interface ISettingService
    {
        Setting Save(Setting setting);
        List<Setting> GetAll();
        Setting? GetSetting(string name);
        SecuritySetting GetSecuritySetting(string name);
    }
}
