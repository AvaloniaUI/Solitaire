namespace Solitaire.Controls;

/// <summary>
/// The offset mode - how we offset individual cards in a stack.
/// </summary>
public enum OffsetMode
{
    /// <summary>
    /// Offset every card.
    /// </summary>
    EveryCard,

    /// <summary>
    /// Offset every Nth card.
    /// </summary>
    EveryNthCard,

    /// <summary>
    /// Offset only the top N cards.
    /// </summary>
    TopNCards,

    /// <summary>
    /// Offset only the bottom N cards.
    /// </summary>
    BottomNCards,

    /// <summary>
    /// Use the offset values specified in the playing card class (see
    /// PlayingCardViewModel.FaceDownOffset and PlayingCardViewModel.FaceUpOffset).
    /// </summary>
    UseCardValues
}