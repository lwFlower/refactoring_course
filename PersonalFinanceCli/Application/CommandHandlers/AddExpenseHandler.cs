using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddExpenseHandler(
    ITransactionRepository transactionRepository,
    ICardRepository cardRepository,
    IClock clock)
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICardRepository _cardRepository = cardRepository;
    private readonly IClock _clock = clock;

    private static void ValidateInput(decimal amount, string category)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Amount must be > 0.");
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            throw new InvalidOperationException("Category cannot be empty.");
        }
    }

    private int ResolveCardId(int? cardId)
    {
        if (cardId.HasValue)
        {
            var byId = _cardRepository.GetById(cardId.Value) ?? throw new InvalidOperationException("Card not found.");
            return byId.Id;
        }

        var defaultByStore = _cardRepository.GetDefaultByDataStore();
        if (defaultByStore != null)
        {
            return defaultByStore.Id;
        }

        var first = _cardRepository.GetFirst() ?? throw new InvalidOperationException("No cards available.");
        return first.Id;
    }

    public Transaction Handle(decimal amount, string category, int? cardId, DateOnly? date, string? note)
    {
        ValidateInput(amount, category);

        int resolvedCardId = ResolveCardId(cardId);

        var trx = new Transaction
        {
            CardId = resolvedCardId,
            Amount = amount,
            Category = category,
            Date = date ?? _clock.Today,
            Note = note,
            Type = TransactionType.Expense
        };

        return _transactionRepository.Add(trx);
    }
}
