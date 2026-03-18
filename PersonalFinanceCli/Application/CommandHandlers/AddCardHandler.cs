using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class AddCardHandler(ICardRepository cardRepository)
{
    private readonly ICardRepository _cardRepository = cardRepository;

    private static Currency ValidateAndParse(string name, string currencyRaw)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Card name cannot be empty.");
        }

        if (!Enum.TryParse<Currency>(currencyRaw, true, out var currency))
        {
            throw new InvalidOperationException("Unknown currency. Allowed: RUB, EUR.");
        }

        return currency;
    }

    public Card Handle(string name, string currencyRaw, decimal? initialBalance)
    {
        Currency currency = ValidateAndParse(name, currencyRaw);

        var card = new Card
        {
            Name = name,
            Currency = currency,
            InitialBalance = initialBalance ?? 0m,
            IsDefault = _cardRepository.GetAll().Count == 0
        };

        return _cardRepository.Add(card);
    }
}
