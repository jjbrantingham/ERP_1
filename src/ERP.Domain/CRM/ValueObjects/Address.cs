using ERP.Domain.Common;

namespace ERP.Domain.CRM.ValueObjects;

/// <summary>
/// Value object representing a physical address.
/// </summary>
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string? Street2 { get; private set; }
    public string City { get; private set; }
    public string? StateProvince { get; private set; }
    public string? PostalCode { get; private set; }
    public string Country { get; private set; }

    private Address()
    {
        Street = string.Empty;
        City = string.Empty;
        Country = string.Empty;
    }

    public Address(
        string street,
        string city,
        string country,
        string? street2 = null,
        string? stateProvince = null,
        string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        Street = street.Trim();
        Street2 = street2?.Trim();
        City = city.Trim();
        StateProvince = stateProvince?.Trim();
        PostalCode = postalCode?.Trim();
        Country = country.Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return Street2 ?? string.Empty;
        yield return City;
        yield return StateProvince ?? string.Empty;
        yield return PostalCode ?? string.Empty;
        yield return Country;
    }

    public override string ToString()
    {
        var parts = new List<string> { Street };
        if (!string.IsNullOrWhiteSpace(Street2)) parts.Add(Street2);
        parts.Add(City);
        if (!string.IsNullOrWhiteSpace(StateProvince)) parts.Add(StateProvince);
        if (!string.IsNullOrWhiteSpace(PostalCode)) parts.Add(PostalCode);
        parts.Add(Country);
        return string.Join(", ", parts);
    }
}
