using SmartWorkz.StarterKitMVC.Infrastructure.Services;
using Xunit;

namespace SmartWorkz.StarterKitMVC.Tests.Unit;

public class PasswordHasherTests
{
    [Fact]
    public void Verify_ShouldReturnTrue_ForCorrectPassword()
    {
        var hasher = new PasswordHasher();
        string password = "TestPassword123!";
        string storedHash = "k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=";

        var result = hasher.Verify(password, storedHash);

        Assert.True(result, "Hash verification failed for TestPassword123!");
    }

    [Fact]
    public void Hash_ShouldGenerateValidHash()
    {
        var hasher = new PasswordHasher();
        string password = "TestPassword123!";

        var hash = hasher.Hash(password);

        var verified = hasher.Verify(password, hash);
        Assert.True(verified);
    }
}
