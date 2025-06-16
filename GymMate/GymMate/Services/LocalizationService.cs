using System.Globalization;
using Microsoft.Maui.Storage;
using System.Linq;

namespace GymMate.Services;

public interface ILocalizationService
{
    IEnumerable<(string code, string name)> Supported { get; }
    string CurrentCode { get; }
    Task SetCultureAsync(string code);
}

public class LocalizationService : ILocalizationService
{
    private readonly IPreferences _preferences;
    private readonly Dictionary<string, ResourceDictionary> _dicts = new();
    private ResourceDictionary? _current;

    public LocalizationService(IPreferences preferences)
    {
        _preferences = preferences;
        foreach (var md in Application.Current.Resources.MergedDictionaries.ToList())
        {
            if (md.ContainsKey("en")) _dicts["en"] = md;
            else if (md.ContainsKey("es")) _dicts["es"] = md;
        }
        var code = _preferences.Get("LanguageCode", null) ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        if (_dicts.TryGetValue(code, out var dict))
        {
            _current = dict;
        }
    }

    public IEnumerable<(string code, string name)> Supported =>
        _dicts.Select(d => (d.Key, (string)d.Value[d.Key]!));

    public string CurrentCode => _current != null && _dicts.ContainsValue(_current)
        ? _dicts.First(k => k.Value == _current).Key
        : "en";

    public Task SetCultureAsync(string code)
    {
        if (_dicts.TryGetValue(code, out var dict))
        {
            if (_current != null)
                Application.Current.Resources.MergedDictionaries.Remove(_current);
            Application.Current.Resources.MergedDictionaries.Add(dict);
            _current = dict;
            CultureInfo.DefaultThreadCurrentUICulture =
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(code);
            _preferences.Set("LanguageCode", code);
        }
        return Task.CompletedTask;
    }
}
