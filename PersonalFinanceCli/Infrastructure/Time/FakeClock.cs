namespace PersonalFinanceCli.Infrastructure.Time;

public sealed class FakeClock(DateOnly today) : IClock
{
    public DateOnly Today { get; set; } = today;
}
