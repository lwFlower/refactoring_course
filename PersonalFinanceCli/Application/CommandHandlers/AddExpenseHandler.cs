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
    private readonly AddTransactionHandler _addTransactionHandler = new(transactionRepository, cardRepository, clock);

    public Transaction Handle(decimal amount, string category, int? cardId, DateOnly? date, string? note)
    {
        return _addTransactionHandler.Handle(TransactionType.Expense, amount, category, cardId, date, note);
    }
}
