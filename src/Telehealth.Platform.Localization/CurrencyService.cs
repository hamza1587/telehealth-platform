namespace Telehealth.Platform.Localization;

public class CurrencyService : ICurrencyService
{
    private readonly Dictionary<string, decimal> _exchangeRates;

    public CurrencyService()
    {
        _exchangeRates = new Dictionary<string, decimal>
        {
            { "EUR", 1.0m },
            { "GBP", 0.85m },
            { "PLN", 4.28m },
            { "SEK", 11.23m },
            { "DKK", 7.45m },
            { "NOK", 11.45m },
            { "CHF", 0.96m },
            { "CZK", 24.15m },
            { "HUF", 395.0m },
            { "BGN", 1.96m },
            { "RON", 4.97m },
            { "HRK", 7.42m }
        };
    }

    public CurrencyConversion Convert(decimal amount, string fromCurrency, string toCurrency)
    {
        if (fromCurrency == toCurrency)
        {
            return new CurrencyConversion(amount, fromCurrency, toCurrency, amount, 1.0m);
        }

        var fromRate = _exchangeRates.GetValueOrDefault(fromCurrency.ToUpper(), 1.0m);
        var toRate = _exchangeRates.GetValueOrDefault(toCurrency.ToUpper(), 1.0m);

        var convertedAmount = amount * (fromRate / toRate);
        var rate = fromRate / toRate;

        return new CurrencyConversion(amount, fromCurrency, toCurrency, convertedAmount, rate);
    }

    public List<CurrencyInfo> GetSupportedCurrencies()
    {
        return _exchangeRates.Select(kvp => new CurrencyInfo(kvp.Key, GetCurrencyName(kvp.Key), GetCurrencySymbol(kvp.Key))).ToList();
    }

    public bool IsCurrencySupported(string currencyCode)
    {
        return _exchangeRates.ContainsKey(currencyCode.ToUpperInvariant());
    }

    private string GetCurrencyName(string code)
    {
        return code.ToUpperInvariant() switch
        {
            "EUR" => "Euro",
            "GBP" => "British Pound",
            "PLN" => "Polish Zloty",
            "SEK" => "Swedish Krona",
            "DKK" => "Danish Krone",
            "NOK" => "Norwegian Krone",
            "CHF" => "Swiss Franc",
            "CZK" => "Czech Koruna",
            "HUF" => "Hungarian Forint",
            "BGN" => "Bulgarian Lev",
            "RON" => "Romanian Leu",
            "HRK" => "Croatian Kuna",
            _ => code
        };
    }

    private string GetCurrencySymbol(string code)
    {
        return code.ToUpperInvariant() switch
        {
            "EUR" => "€",
            "GBP" => "£",
            "PLN" => "zł",
            "SEK" => "kr",
            "DKK" => "kr",
            "NOK" => "kr",
            "CHF" => "CHF",
            "CZK" => "Kč",
            "HUF" => "Ft",
            "BGN" => "лв",
            "RON" => "lei",
            "HRK" => "kn",
            _ => code
        };
    }
}

public interface ICurrencyService
{
    CurrencyConversion Convert(decimal amount, string fromCurrency, string toCurrency);
    List<CurrencyInfo> GetSupportedCurrencies();
    bool IsCurrencySupported(string currencyCode);
}

public record CurrencyConversion(decimal Amount, string FromCurrency, string ToCurrency, decimal ConvertedAmount, decimal Rate);

public record CurrencyInfo(string Code, string Name, string Symbol);