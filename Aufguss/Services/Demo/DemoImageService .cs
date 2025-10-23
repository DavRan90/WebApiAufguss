using Aufguss.Models;
using Aufguss.Pages;
using Aufguss.Services.Interface;
using System.Xml.Linq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Aufguss.Services.Demo
{
    public class DemoImageService : IImageService
    {
        public Task<List<string>> GetImageListAsync()
        {
            // Assuming images are in wwwroot/images/
            var demoImages = new List<string>
        {
            "images/aufguss.jpg",
            "images/strand.jpg",
        };

            return Task.FromResult(demoImages);
        }
    }
}
