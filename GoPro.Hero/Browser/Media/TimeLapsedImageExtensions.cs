using System.Threading.Tasks;

namespace GoPro.Hero.Browser.Media
{
    public static class TimeLapsedImageExtensions
    {
       

        public static async Task<IMediaBrowser> DeleteAsync(this TimeLapsedImage image)
        {
            for (var i = image.Start; i <= image.End; i++)
            {
                var indexName = image.IndexName(i);
                await image.DeleteFile(indexName);
            }
            return image.Browser;
        }
    }
}
