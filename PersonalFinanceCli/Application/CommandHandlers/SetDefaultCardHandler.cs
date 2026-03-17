using PersonalFinanceCli.Application.Repositories;

namespace PersonalFinanceCli.Application.CommandHandlers;

public sealed class SetDefaultCardHandler(ICardRepository cardRepository)
{
    private readonly ICardRepository _cardRepository = cardRepository;

    public void Handle(int cardId)
    {
        var card = _cardRepository.GetById(cardId);
        if (card is null)
        {
            throw new InvalidOperationException("Card not found.");
        }

        _cardRepository.SetDefault(cardId);
    }
}
