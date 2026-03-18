using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Domain.Services;

public sealed class DailyReportService(
    ICardRepository cardRepository,
    ITransactionRepository transactionRepository,
    ILimitRepository limitRepository)
{
    private readonly ICardRepository _cardRepository = cardRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ILimitRepository _limitRepository = limitRepository;

    public DailyReport Generate(DateOnly date)
    {
        var cards = _cardRepository.GetAll();
        var allTransactions = _transactionRepository.GetAll();

        var currency = GetReportCurrency(cards);
        var targetCardIds = cards.Where(c => c.Currency == currency).Select(c => c.Id).ToHashSet();

        var (income, expense, categoryTotals) = CalculateDailyStats(allTransactions, targetCardIds, date);

        var limit = _limitRepository.GetByDate(date);

        var balances = CalculateBalances(cards, allTransactions);

        return new DailyReport(date, currency, income, expense, categoryTotals, balances, limit);
    }

    private Currency GetReportCurrency(IEnumerable<Card> cards)
    {
        return cards.FirstOrDefault(c => c.IsDefault)?.Currency
            ?? cards.FirstOrDefault()?.Currency
            ?? Currency.RUB;
    }

    private (decimal Income, decimal Expense, Dictionary<string, decimal> CategoryTotals) CalculateDailyStats(
        IEnumerable<Transaction> transactions, 
        HashSet<int> cardIds,
        DateOnly date)
    {
        decimal income = 0m;
        decimal expense = 0m;
        var categoryTotals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var dailyTransactions = transactions.Where(t => t.Date == date && cardIds.Contains(t.CardId));

        foreach (var transaction in dailyTransactions)
        {
            if (transaction.Type == TransactionType.Income)
            {
                income += transaction.Amount;
            }
            else 
            {
                expense += transaction.Amount;
                
                var currentTotal = categoryTotals.GetValueOrDefault(transaction.Category);
                categoryTotals[transaction.Category] = currentTotal + transaction.Amount;
            }
        }

        return (income, expense, categoryTotals);
    }

    private List<CardBalanceLine> CalculateBalances(IEnumerable<Card> cards, IEnumerable<Transaction> allTransactions)
    {
        var balanceChanges = new Dictionary<int, decimal>(); 
        
        foreach (var transaction in allTransactions)
        {
            var amount = transaction.Type == TransactionType.Income 
                ? transaction.Amount 
                : -transaction.Amount;

            var currentChange = balanceChanges.GetValueOrDefault(transaction.CardId);
            balanceChanges[transaction.CardId] = currentChange + amount;
        }

        var balances = new List<CardBalanceLine>();
        foreach (var card in cards)
        {
            var finalBalance = card.InitialBalance + balanceChanges.GetValueOrDefault(card.Id);
            
            balances.Add(new CardBalanceLine(card.Id, card.Name, card.IsDefault, finalBalance, card.Currency));
        }

        return balances;
    }
}

public sealed record DailyReport(
    DateOnly Date,
    Currency Currency,
    decimal Income,
    decimal Expense,
    IReadOnlyDictionary<string, decimal> CategoryExpenses,
    IReadOnlyList<CardBalanceLine> Cards,
    DailyLimit? Limit);

public sealed record CardBalanceLine(int CardId, string CardName, bool IsDefault, decimal Balance, Currency Currency);
