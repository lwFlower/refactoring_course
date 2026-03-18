using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class SetDailyLimitHandler(ILimitRepository limitRepository, ICardRepository cardRepository, IClock clock)
{
    private readonly ILimitRepository _limitRepository = limitRepository;
    private readonly ICardRepository _cardRepository = cardRepository;
    private readonly IClock _clock = clock;

public void Handle(decimal amount)
    {
        var cards = _cardRepository.GetAll();
        ValidateLimitAmount(amount);
        EnsureCardsExist(cards);

        var currency = _cardRepository.GetDefault()?.Currency
            ?? _cardRepository.GetFirst()?.Currency
            ?? cards[0].Currency;

        _limitRepository.Upsert(_clock.Today, amount, currency);
    }

    private static void ValidateLimitAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Limit must be > 0.");
        }
    }

    private static void EnsureCardsExist(IReadOnlyList<Domain.Entities.Card> cards)
    {
        if (cards.Count == 0)
        {
            throw new InvalidOperationException("Cannot set limit without cards.");
        }
    }
}
