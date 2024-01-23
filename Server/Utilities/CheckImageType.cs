using Microsoft.AspNetCore.Components.Forms;
using System.Linq;

namespace Server.Utilities
{
    public class CheckImageType
    {
        public bool IsImageFile(IBrowserFile file)
        {
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(file.Name).ToLowerInvariant();
            return permittedExtensions.Contains(extension);
        }
    }
}
