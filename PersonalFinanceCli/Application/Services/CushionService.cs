using PersonalFinanceCli.Application.Repositories;
using PersonalFinanceCli.Domain.Entities;
using PersonalFinanceCli.Domain.ValueObjects;

namespace PersonalFinanceCli.Application.Services;

public sealed class CushionService(ICardRepository cardRepository)
{
    private const string CushionFullName = "Финансовая подушка";
    private const string CushionKeyword = "подушка";

    private readonly ICardRepository _cardRepository = cardRepository;

    public Card? FindCushion()
    {
        var cards = _cardRepository.GetAll();

        return cards.FirstOrDefault(c => c.Name == CushionFullName)
            ?? cards.FirstOrDefault(c => c.IsCushion)
            ?? cards.FirstOrDefault(c => c.Name.Contains(CushionKeyword, StringComparison.OrdinalIgnoreCase));
    }

    public Card? FindCushionByName()
    {
        var cards = _cardRepository.GetAll();
        return cards.FirstOrDefault(c => c.Name == "Финансовая подушка");
    }

    public Card? FindCushionByContains()
    {
        var cards = _cardRepository.GetAll();
        return cards.FirstOrDefault(c => c.Name.Contains("подушка", StringComparison.OrdinalIgnoreCase));
    }

    public Card CreateCushion(Currency currency)
    {
        var existing = FindCushion();
        if (existing != null)
        {
            return existing;
        }

        return _cardRepository.Add(new Card
        {
            Name = CushionFullName,
            Currency = currency,
            InitialBalance = 0m,
            IsDefault = false,
            IsCushion = true
        });
    }

    public decimal DefaultTransferAmount(decimal incomeAmount, string category)
    {
        var hasSalaryWord = category.Contains("Зарплата", StringComparison.OrdinalIgnoreCase);

        if (incomeAmount < 10m) return 1m;

        var rate = hasSalaryWord ? 0.20m : 0.10m;
        return Floor2(incomeAmount * rate);
    }

    //TODO: как будто бы Floor2 не связан напрямую с cushion, в своём проекте вынесла бы в utils
    //Но тут оставляю, т.к. не разбираюсь сильно в архитектуре проектов C#
    public static decimal Floor2(decimal value)
    {
        return Math.Floor(value * 100m) / 100m;
    }
}
