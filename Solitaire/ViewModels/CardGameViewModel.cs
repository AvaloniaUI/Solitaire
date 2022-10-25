using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Solitaire.ViewModels.Pages;

namespace Solitaire.ViewModels;

/// <summary>
/// Base class for a ViewModel for a card game.
/// </summary>
public abstract partial class CardGameViewModel : ViewModelBase
{
    public abstract string? GameName { get; }
    
    #if DEBUG
    protected CardGameViewModel()
    {
    }
    #endif 

    /// <summary>
    /// Initializes a new instance of the <see cref="CardGameViewModel"/> class.
    /// </summary>
    protected CardGameViewModel(CasinoViewModel casinoViewModel)
    {
        NavigateToCasinoCommand =
            new RelayCommand(() =>
            {
                if (Moves > 0)
                {
                    _gameStats.UpdateStatistics();
                    casinoViewModel.Save();
                }

                casinoViewModel.CurrentView = casinoViewModel.TitleInstance;
            });

        //  Set up the timer.
        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += timer_Tick;
    }

    public abstract IList<PlayingCardViewModel>? GetCardCollection(PlayingCardViewModel card);


    public abstract bool CheckAndMoveCard(IList<PlayingCardViewModel> from,
        IList<PlayingCardViewModel> to,
        PlayingCardViewModel card,
        bool checkOnly = false);

    /// <summary>
    /// Deals a new game.
    /// </summary>
    protected void ResetInternalState()
    {
        //  Stop the timer and reset the game data.
        StopTimer();
        ElapsedTime = TimeSpan.FromSeconds(0);
        Moves = 0;
        Score = 0;
        IsGameWon = false;
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    protected void StartTimer()
    {
        _lastTick = DateTime.Now;
        _timer.Start();
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    protected void StopTimer()
    {
        _timer.Stop();
    }

    /// <summary>
    /// Handles the Tick event of the timer control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void timer_Tick(object? sender, EventArgs e)
    {
        //  Get the time, update the elapsed time, record the last tick.
        var timeNow = DateTime.Now;
        ElapsedTime += timeNow - _lastTick;
        _lastTick = timeNow;
    }

    /// <summary>
    /// Fires the game won event.
    /// </summary>
    protected void FireGameWonEvent()
    {
        _gameStats.UpdateStatistics();

        var wonEvent = GameWon;
        if (wonEvent is not { })
            wonEvent?.Invoke();
    }

    /// <summary>
    /// The timer for recording the time spent in a game.
    /// </summary>
    private readonly DispatcherTimer _timer = new();

    /// <summary>
    /// The time of the last tick.
    /// </summary>
    private DateTime _lastTick;

    [ObservableProperty] private int _score;

    [ObservableProperty] private TimeSpan _elapsedTime;

    [ObservableProperty] private int _moves;

    [ObservableProperty] private bool _isGameWon;
    private GameStatisticsViewModel _gameStats = null!;

    /// <summary>
    /// Gets the go to casino command.
    /// </summary>
    /// <value>The go to casino command.</value>
    public ICommand? NavigateToCasinoCommand { get; }

    /// <summary>
    /// Gets the deal new game command.
    /// </summary>
    /// <value>The deal new game command.</value>
    public ICommand? NewGameCommand { get; protected set; }

    /// <summary>
    /// Occurs when the game is won.
    /// </summary>
    public event Action GameWon = null!;

    public abstract void ResetGame();

    public void RegisterStatsInstance(GameStatisticsViewModel gameStatsInstance)
    {
        _gameStats = gameStatsInstance;
    }
}