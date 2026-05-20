using WaterTracker.Api.Common;

namespace WaterTracker.Tests;

public class ResultTests
{
    [Fact]
    public void SuccessResult_ShouldSetSuccessAndData()
    {
        var result = Result<string>.SuccessResult("hello");

        Assert.True(result.Success);
        Assert.Equal("hello", result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_ShouldSetFailureAndError()
    {
        var result = Result<int>.Failure("something went wrong");

        Assert.False(result.Success);
        Assert.Equal("something went wrong", result.Error);
    }

    [Fact]
    public void SuccessBool_ShouldSetSuccessOnly()
    {
        var result = Result<bool>.SuccessBool();

        Assert.True(result.Success);
        Assert.Null(result.Error);
    }
}
