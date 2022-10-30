using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Solitaire.Controls;
using Solitaire.Models;

namespace Solitaire.ViewModels.Pages;

public record StacksMetadata(string Name,
    Point StackOrigin,
    ObservableCollection<PlayingCardViewModel> Collection,
    double FaceDownOffset, 
    double FaceUpOffset, 
    OffsetMode OffsetMode, 
    ICommand? CommandOnCardClick,
    DrawMode NValue,
    bool IsHomeStack);