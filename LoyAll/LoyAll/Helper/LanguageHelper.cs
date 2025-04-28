using LoyAll.Resources.Strings;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace LoyAll.Helper
{
    public class LanguageHelper : INotifyPropertyChanged
    {
        private const string LanguageKey = "AppLanguage";
        private static LanguageHelper _instance;

        public event PropertyChangedEventHandler PropertyChanged;

        public static LanguageHelper Instance => _instance ??= new LanguageHelper();

        public string this[string key]
        {
            get
            {
                try
                {
                    var value = AppResources.ResourceManager.GetString(key, AppResources.Culture);
                    return value ?? $"[MISSING:{key}]";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting resource '{key}': {ex}");
                    return $"[ERROR:{key}]";
                }
            }
        }

        private LanguageHelper()
        {
            Initialize();
        }

        public void Initialize()
        {
            string languageCode = Preferences.Get(LanguageKey, null);

            if (string.IsNullOrEmpty(languageCode))
            {
                var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                languageCode = culture switch
                {
                    "pl" => "pl",
                    "de" => "de",
                    _ => "en"     
                };
                Preferences.Set(LanguageKey, languageCode);
            }

            AppResources.Culture = new CultureInfo(languageCode);
        }

        public void ChangeLanguage(string language)
        {
            string languageCode = language switch
            {
                "Polski" => "pl",
                "English" => "en",
                "Deutsch" => "de", 
                _ => "en"
            };

            Preferences.Set(LanguageKey, languageCode);
            AppResources.Culture = new CultureInfo(languageCode);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public string GetCurrentLanguage()
        {
            string languageCode = Preferences.Get(LanguageKey, "en");
            return languageCode switch
            {
                "pl" => "Polski",
                "de" => "Deutsch", 
                _ => "English"
            };
        }
    }
}