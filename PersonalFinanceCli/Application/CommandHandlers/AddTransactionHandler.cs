using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;
using PersonalFinanceCli.Infrastructure.Time;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddTransactionHandler(
    ITransactionRepository transactionRepository,
    ICardRepository cardRepository,
    IClock clock)
{
    public const string TransferToCushion = "Transfer to cushion";
    public const string TransferFromIncome = "Transfer from income";

    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICardRepository _cardRepository = cardRepository;
    private readonly IClock _clock = clock;

    public Transaction Handle(
        TransactionType type,
        decimal amount,
        string category,
        int? cardId,
        DateOnly? date,
        string? note)
    {
        Validate(amount, category);

        var resolvedCardId = EnsureCardSelectedFallback(cardId, type);
        EnsureCardExists(resolvedCardId);

        var trx = new Transaction
        {
            CardId = resolvedCardId,
            Amount = amount,
            Category = category,
            Date = date ?? _clock.Today,
            Note = note,
            Type = type
        };

        return _transactionRepository.Add(trx);
    }

    private static void Validate(decimal amount, string category)
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

    private void EnsureCardExists(int cardId)
    {
        _ = _cardRepository.GetById(cardId) ?? throw new InvalidOperationException("Card not found.");
    }

    public int EnsureCardSelectedFallback(int? cardId, TransactionType type)
    {
        if (cardId.HasValue)
        {
            var byId = _cardRepository.GetById(cardId.Value) ?? throw new InvalidOperationException("Card not found.");
            return byId.Id;
        }

        return type == TransactionType.Expense
            ? (_cardRepository.GetDefaultByDataStore()?.Id ?? GetFirstCardIdOrThrow())
            : (_cardRepository.GetDefault()?.Id ?? GetFirstCardIdOrThrow());
    }

    private int GetFirstCardIdOrThrow()
    {
        return _cardRepository.GetFirst()?.Id ?? throw new InvalidOperationException("No cards available.");
    }

    public int ResolveCardId(int? cardId)
    {
        return EnsureCardSelectedFallback(cardId, TransactionType.Income);
    }

    public Card? FindCushionCardLoose()
    {
        var cards = _cardRepository.GetAll();
        var byFlag = cards.FirstOrDefault(c => c.IsCushion);
        if (byFlag != null)
        {
            return byFlag;
        }

        var exact = cards.FirstOrDefault(c => c.Name == "Финансовая подушка");
        if (exact != null)
        {
            return exact;
        }

        return cards.FirstOrDefault(c => c.Name.Contains("подушка"));
    }

    public void AddTransferPair(int fromCardId, int cushionCardId, decimal amount, DateOnly? date)
    {
        var transferDate = date ?? _clock.Today;

        _transactionRepository.Add(new Transaction
        {
            CardId = fromCardId,
            Amount = amount,
            Category = TransferToCushion,
            Date = transferDate,
            Note = "auto",
            Type = TransactionType.Expense
        });

        _transactionRepository.Add(new Transaction
        {
            CardId = cushionCardId,
            Amount = amount,
            Category = TransferFromIncome,
            Date = transferDate,
            Note = "auto",
            Type = TransactionType.Income
        });
    }
}
