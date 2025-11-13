namespace AasDemoapp.Settings;

public class ImpressumSettings
{
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = "Deutschland";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
