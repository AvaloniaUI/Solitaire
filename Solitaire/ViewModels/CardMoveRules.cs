using System.Collections.Generic;
using System.Data;
using System.Linq;
using Solitaire.Models;
using Solitaire.Utils;

namespace Solitaire.ViewModels;

internal static class CardMoveRules
{
    public static CardSuit GetFoundationSuit(IList<PlayingCardViewModel> stack,
        IReadOnlyList<BatchObservableCollection<PlayingCardViewModel>> foundations)
    {
        if (ReferenceEquals(stack, foundations[0]))
            return CardSuit.Hearts;
        if (ReferenceEquals(stack, foundations[1]))
            return CardSuit.Clubs;
        if (ReferenceEquals(stack, foundations[2]))
            return CardSuit.Diamonds;
        if (ReferenceEquals(stack, foundations[3]))
            return CardSuit.Spades;
        throw new InvalidConstraintException();
    }

    public static bool CanPlaceOnTableau(IList<PlayingCardViewModel> destination,
        PlayingCardViewModel card, bool emptyAllowed)
    {
        return (destination.Count == 0 && emptyAllowed) ||
            (destination.Count > 0 && destination.Last().Colour != card.Colour && destination.Last().Value == card.Value + 1);
    }

    public static bool CanBuildFoundation(IList<PlayingCardViewModel> destination,
        PlayingCardViewModel card, IReadOnlyList<BatchObservableCollection<PlayingCardViewModel>> foundations,
        bool requireTopSuit)
    {
        return GetFoundationSuit(destination, foundations) == card.Suit &&
            ((destination.Count == 0 && card.Value == 0) ||
             (destination.Count > 0 && (!requireTopSuit || destination.Last().Suit == card.Suit) && destination.Last().Value == card.Value - 1));
    }
}
