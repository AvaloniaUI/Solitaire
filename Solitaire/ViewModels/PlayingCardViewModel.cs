using CommunityToolkit.Mvvm.ComponentModel;
using Solitaire.Models;

namespace Solitaire.ViewModels;

/// <summary>
/// A card and its current game state.
/// </summary>
public partial class PlayingCardViewModel : ViewModelBase
{
    public CardGameViewModel? CardGameInstance { get; }

    public PlayingCardViewModel(CardGameViewModel? cardGameInstance)
    {
        CardGameInstance = cardGameInstance;
    }

    public CardSuit Suit
    {
        get
        {
            var enumVal = (int)CardType;
            return enumVal switch
            {
                < 13 => CardSuit.Hearts,
                < 26 => CardSuit.Diamonds,
                < 39 => CardSuit.Clubs,
                _ => CardSuit.Spades
            };
        }
    }

    public int Value =>
        // CardType stores 13 cards per suit.
        (int)CardType % 13;

    public CardColour Colour =>
        // CardType lists the red suits before the black suits.
        (int)CardType < 26 ? CardColour.Red : CardColour.Black;

    [ObservableProperty] private CardType _cardType = CardType.SA;
    [ObservableProperty] private bool _isFaceDown;
    [ObservableProperty] private bool _isPlayable;
    [ObservableProperty] private double _faceDownOffset;
    [ObservableProperty] private double _faceUpOffset;

    public void Reset()
    {
        IsPlayable = false;
        IsFaceDown = true;
        FaceDownOffset = 0;
        FaceUpOffset = 0;
    }

}
