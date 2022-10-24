using System.Threading.Tasks;

namespace Solitaire.ViewModels;

public interface IRuntimeStorageProvider<T>
{
    Task SaveObject(T obj);
    Task<T?> LoadObject();
}