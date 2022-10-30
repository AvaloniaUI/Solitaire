using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Converters;
using Solitaire.Models;

namespace Solitaire.ViewModels;

/// <summary>
/// The Playing Card represents a Card played in a game - so as
/// well as the card type it also has the face down property etc.
/// </summary>
public partial class PlayingCardViewModel : ViewModelBase, IEquatable<PlayingCardViewModel>
{
    public CardGameViewModel? CardGameInstance { get; }

    public PlayingCardViewModel(CardGameViewModel? cardGameInstance)
    {
        CardGameInstance = cardGameInstance;
    }
        
    /// <summary>
    /// Gets the card suit.
    /// </summary> 
    /// <value>The card suit.</value>
    public CardSuit Suit
    {
        get
        {
            //  The suit can be worked out from the numeric value of the CardType enum.
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

    /// <summary>
    /// Gets the card value.
    /// </summary>
    /// <value>The card value.</value>
    public int Value =>
        //  The CardType enum has 13 cards in each suit.
        (int)CardType % 13;

    /// <summary>
    /// Gets the card colour.
    /// </summary>
    /// <value>The card colour.</value>
    public CardColour Colour =>
        //  The first two suits in the CardType enum are red, the last two are black.
        (int)CardType < 26 ? CardColour.Red : CardColour.Black;

    [ObservableProperty]  private CardType _cardType  = CardType.SA;
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

    /// <inheritdoc />
    public bool Equals(PlayingCardViewModel? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _cardType == other._cardType;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((PlayingCardViewModel) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine((int) _cardType);
    }
}