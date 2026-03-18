using PersonalFinanceCli.Application.Repositories;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class SetDefaultCardHandler(ICardRepository cardRepository)
{
    private readonly ICardRepository _cardRepository = cardRepository;

    public void Handle(int cardId)
    {
         _ = _cardRepository.GetById(cardId) ?? throw new InvalidOperationException("Card not found.");

        _cardRepository.SetDefault(cardId);
    }
}
