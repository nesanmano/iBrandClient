using System.Threading.Tasks;
using GoPro.Hero.Commands;

namespace GoPro.Hero.Browser.Media
{
    public static class MediaExtensions
    {
       

        internal static async Task DeleteFile(this IMedia media, string name)
        {
            var path = string.Format("{0}/{1}", media.Browser.Destination, name);
            await media.Browser.Camera.PrepareCommand<CommandCameraDeleteFile>().Set(path).ExecuteAsync();
        }
    }
}
