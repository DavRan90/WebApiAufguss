namespace Aufguss.Services.Interface
{
    public interface IRefreshService
    {
        Task<bool> TryRefreshAsync();
    }
}
