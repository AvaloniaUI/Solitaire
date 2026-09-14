using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Solitaire.Models;
using Solitaire.ViewModels;

namespace Solitaire.Controls;

// Prepare card turns and group shadows with card animations.
internal sealed class CardRenderPreparation : IDisposable
{
    private readonly List<CardMotionFlight> _flights = new();
    private readonly CancellationToken _token;
    private readonly CancellationTokenRegistration _cancellation;
    public Canvas Scene { get; } = CreateCards();

    public CardRenderPreparation(CancellationToken token)
    {
        _token = token;
        _cancellation = token.Register(Cancel);
    }

    public async Task Animate()
    {
        _token.ThrowIfCancellationRequested();
        var cards = Scene.Children.OfType<PlayingCard>().ToArray();
        _flights.Add(CreateFlight(cards[..1], false));
        _flights.Add(CreateFlight(cards[1..2], false));
        _flights.Add(CreateFlight(cards[2..], false));
        await Task.WhenAll(_flights.Select(flight => flight.Animate(0, RenderPreparation.SampleDuration))).WaitAsync(_token);
        _token.ThrowIfCancellationRequested();
        for (var index = 2; index < cards.Length; index++)
            cards[index].SetPose(cards[index].Pose with { Position = cards[index].Pose.Position + new Vector(0, (index - 2) * 30) });
        var held = CreateFlight(cards[2..], true);
        _flights.Add(held);
        // Keep the cards lifted until the final frame completes.
        await held.Animate(0, RenderPreparation.SampleDuration).WaitAsync(_token);
    }

    private CardMotionFlight CreateFlight(PlayingCard[] cards, bool held)
    {
        var shadow = new CardCastShadow();
        Scene.Children.Insert(0, shadow);
        var turn = cards.Length == 1;
        var targets = cards.Select(card => new CardMotionTarget(card,
            card.Pose.Position + (held ? default : new Vector(0, 40)),
            turn ? 1 - card.Pose.Turn : card.Pose.Turn, turn)).ToArray();
        return new CardMotionFlight(targets, new CardPoseVelocity?[cards.Length], shadow, held, !turn, _ =>
        {
            foreach (var card in cards)
                card.Land();
        });
    }

    private static Canvas CreateCards()
    {
        var scene = new Canvas { Width = 620, Height = 360, Background = Brushes.Transparent };
        var template = (IDataTemplate)Application.Current!.FindResource("PlayingCardDataTemplate")!;
        // Each group in the Spider deal has thirteen overlapping cards.
        for (var index = 0; index < 15; index++)
        {
            var pose = new CardPose(new Vector(20 + Math.Min(index, 2) * 180, 30), index == 0 ? 0 : 1, 0);
            var card = new PlayingCard
            {
                ContentTemplate = template,
                Content = new PlayingCardViewModel(null) { CardType = index == 1 ? CardType.SA : CardType.SQ, IsFaceDown = false }
            };
            card.SetPose(pose);
            scene.Children.Add(card);
        }
        return scene;
    }

    private void Cancel()
    {
        foreach (var flight in _flights)
            flight.Dispose();
    }

    public void Dispose()
    {
        _cancellation.Dispose();
        Cancel();
        _flights.Clear();
        foreach (var card in Scene.Children.OfType<PlayingCard>())
            card.Content = null;
        Scene.Children.Clear();
    }
}
