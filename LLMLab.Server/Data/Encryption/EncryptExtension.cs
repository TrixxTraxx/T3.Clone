using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LLMLab.Server.Data;

public static class EncryptExtension
{
    public static PropertyBuilder<string> HasEncryption(this PropertyBuilder<string> propertyBuilder, IEncryptionProvider provider)
    {
        /*var converter = new ValueConverter<string, string>(
            v => Encrypt(v, provider),
            v => Decrypt(v, provider));*/


        propertyBuilder.HasConversion(
            x => Encrypt(x, provider),
            x => Decrypt(x, provider)
        );
        /*propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(new ValueComparer<string>(
            (l, r) => l == r,
            v => v == null ? 0 : v.GetHashCode(),
            v => Decrypt(Encrypt(v, provider), provider)));*/

        return propertyBuilder;
    }
    private static string Encrypt(string obj, IEncryptionProvider provider)
    {
        return provider.Encrypt(obj);
    }
    private static string Decrypt(string json, IEncryptionProvider provider)
    {
        return provider.Decrypt(json);
    }
}