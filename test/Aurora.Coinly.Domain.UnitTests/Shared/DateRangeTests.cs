namespace Aurora.Coinly.Domain.UnitTests.Shared;

public class DateRangeTests
{
    private static readonly DateOnly Start = new(2026, 1, 1);
    private static readonly DateOnly End = new(2026, 1, 31);

    [Fact]
    public void Create_Should_SetProperties()
    {
        // Arrange

        // Act
        Result<DateRange> result = DateRange.Create(Start, End);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.Start.Should().Be(Start);
        result.Value.End.Should().Be(End);
        result.Value.TotalDays.Should().Be(31);
    }

    [Fact]
    public void Create_Should_Succeed_WhenStartEqualsEnd()
    {
        // Arrange

        // Act
        Result<DateRange> result = DateRange.Create(Start, Start);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Value.TotalDays.Should().Be(1);
    }

    [Fact]
    public void Create_Should_Fail_WhenStartIsAfterEnd()
    {
        // Arrange

        // Act
        Result<DateRange> result = DateRange.Create(End, Start);

        // Assert
        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().Be(DateRangeErrors.StartAfterEnd);
    }
}
