using System.Threading.Tasks;

namespace SolitaireAvalonia.ViewModels;

public interface IRuntimeStorageProvider<T>
{
    Task SaveObject(T obj);
    Task<T?> LoadObject();
}