using Aufguss.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aufguss.Services.Interface
{
    public interface IImageService
    {
        Task<List<string>> GetImageListAsync();
    }
}
