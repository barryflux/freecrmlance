using Freecrmlance.Application.Crm;
using NUnit.Framework;

namespace Freecrmlance.UnitTests;

public sealed class CustomerImportMappingSuggesterTests
{
    [TestCase("Société", CustomerImportField.Name)]
    [TestCase("Company", CustomerImportField.Name)]
    [TestCase("E-mail", CustomerImportField.Email)]
    [TestCase("Portable", CustomerImportField.Phone)]
    [TestCase("N° SIRET", CustomerImportField.Siret)]
    [TestCase("Code Postal", CustomerImportField.PostalCode)]
    [TestCase("Localité", CustomerImportField.City)]
    [TestCase("colonne inconnue", CustomerImportField.Ignore)]
    public void Suggest_recognizes_common_headers(string header, CustomerImportField expected)
        => Assert.That(CustomerImportMappingSuggester.Suggest(header), Is.EqualTo(expected));

    [Test]
    public void Suggest_ignores_accents_case_and_punctuation()
    {
        Assert.That(CustomerImportMappingSuggester.Suggest("  SOCIÉTÉ  "), Is.EqualTo(CustomerImportField.Name));
        Assert.That(CustomerImportMappingSuggester.Suggest("N° TVA"), Is.EqualTo(CustomerImportField.VatNumber));
    }
}
