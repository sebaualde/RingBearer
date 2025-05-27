using RingBearer.Core.Constants;

namespace RingBearer.Tests.CoreTests;
public class AppConstantsTests
{
    [Fact]
    public void FilePath_ShouldHaveExpectedValue()
    {
        Assert.Equal("ghâsh-bûrz-krimp.ring", AppConstants.FileName);
    }

    [Fact]
    public void AppConfigsFilePath_ShouldHaveExpectedValue()
    {
        Assert.Equal("bearer.settings.json", AppConstants.AppConfigsFilePath);
    }

    [Fact]
    public void SaltSize_ShouldBe16()
    {
        Assert.Equal(16, AppConstants.SaltSize);
    }

    [Fact]
    public void KeySize_ShouldBe32()
    {
        Assert.Equal(32, AppConstants.KeySize);
    }

    [Fact]
    public void Iterations_ShouldBe100000()
    {
        Assert.Equal(100_000, AppConstants.Iterations);
    }
}
