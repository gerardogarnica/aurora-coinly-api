namespace Aurora.Coinly.Domain.UnitTests.Budgets;

public class BudgetPeriodServiceTests : BaseTest
{
    [Fact]
    public void GeneratePeriods_Should_ReturnWeeklyPeriods()
    {
        // Arrange
        var service = new BudgetPeriodService();
        var frequency = BudgetFrequency.Weekly;
        var year = DateTime.UtcNow.Year;

        // Act
        var periods = service.GeneratePeriods(frequency, year);

        // Assert
        periods.Should().HaveCount(52);
        periods.Should().AllSatisfy(period =>
        {
            period.Start.Should().BeOnOrAfter(new DateOnly(year, 1, 1));
            period.End.Should().BeOnOrBefore(new DateOnly(year, 12, 31));
        });
    }

    [Fact]
    public void GeneratePeriods_Should_ReturnBiweeklyPeriods()
    {
        // Arrange
        var service = new BudgetPeriodService();
        var frequency = BudgetFrequency.Biweekly;
        var year = DateTime.UtcNow.Year;
        var currentMonth = 0;

        // Act
        var periods = service.GeneratePeriods(frequency, year);

        // Assert
        periods.Should().HaveCount(24);
        for (var i = 0; i < 24; i++)
        {
            if (i % 2 == 0)
            {
                currentMonth++;

                var expectedStart = new DateOnly(year, currentMonth, 1);
                var expectedEnd = new DateOnly(year, currentMonth, 15);

                periods.Should().Contain(period =>
                    period.Start == expectedStart && period.End == expectedEnd);
            }
            else
            {
                var expectedStart = new DateOnly(year, currentMonth, 16);
                var expectedEnd = expectedStart.AddMonths(1).AddDays(-1);

                periods.Should().Contain(period =>
                    period.Start == expectedStart && period.End == expectedEnd);
            }
        }
    }

    [Fact]
    public void GeneratePeriods_Should_ReturnMonthlyPeriods()
    {
        // Arrange
        var service = new BudgetPeriodService();
        var frequency = BudgetFrequency.Monthly;
        var year = DateTime.UtcNow.Year;
        var currentMonth = 0;

        // Act
        var periods = service.GeneratePeriods(frequency, year);

        // Assert
        periods.Should().HaveCount(12);
        for (var i = 0; i < 12; i++)
        {
            currentMonth = i + 1;

            var expectedStart = new DateOnly(year, currentMonth, 1);
            var expectedEnd = expectedStart.AddMonths(1).AddDays(-1);

            periods.Should().Contain(period =>
                period.Start == expectedStart && period.End == expectedEnd);
        }
    }

    [Fact]
    public void GeneratePeriods_Should_ReturnQuarterlyPeriods()
    {
        // Arrange
        var service = new BudgetPeriodService();
        var frequency = BudgetFrequency.Quarterly;
        var year = DateTime.UtcNow.Year;
        var currentMonth = 0;

        // Act
        var periods = service.GeneratePeriods(frequency, year);

        // Assert
        periods.Should().HaveCount(4);
        for (var i = 0; i < 4; i++)
        {
            currentMonth = (i * 3) + 1;

            var expectedStart = new DateOnly(year, currentMonth, 1);
            var expectedEnd = expectedStart.AddMonths(3).AddDays(-1);

            periods.Should().Contain(period =>
                period.Start == expectedStart && period.End == expectedEnd);
        }
    }

    [Fact]
    public void GeneratePeriods_Should_ReturnSemiAnnualPeriods()
    {
        // Arrange
        var service = new BudgetPeriodService();
        var frequency = BudgetFrequency.SemiAnnual;
        var year = DateTime.UtcNow.Year;
        var currentMonth = 0;

        // Act
        var periods = service.GeneratePeriods(frequency, year);

        // Assert
        periods.Should().HaveCount(2);
        for (var i = 0; i < 2; i++)
        {
            currentMonth = (i * 6) + 1;

            var expectedStart = new DateOnly(year, currentMonth, 1);
            var expectedEnd = expectedStart.AddMonths(6).AddDays(-1);

            periods.Should().Contain(period =>
                period.Start == expectedStart && period.End == expectedEnd);
        }
    }

    [Fact]
    public void GeneratePeriods_Should_ReturnAnnualPeriod()
    {
        // Arrange
        var service = new BudgetPeriodService();
        var frequency = BudgetFrequency.Annual;
        var year = DateTime.UtcNow.Year;

        // Act
        var periods = service.GeneratePeriods(frequency, year);

        // Assert
        periods.Should().HaveCount(1);
        var expectedStart = new DateOnly(year, 1, 1);
        var expectedEnd = new DateOnly(year, 12, 31);

        periods.Should().Contain(period =>
            period.Start == expectedStart && period.End == expectedEnd);
    }
}