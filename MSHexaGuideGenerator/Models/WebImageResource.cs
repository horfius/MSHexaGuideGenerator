using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Drawing;
using System.Net;
using System.Drawing.Imaging;

namespace MSHexaGuideGen.Models
{
    public record WebImageResource(string Name, string Url) : IDisposable
    {
        [JsonIgnore]
        public static string LocalFileStorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MSHexaGuideGen", "downloaded_assets");

        [JsonIgnore]
        private const string LocalFileFormat = ".png";

        [JsonIgnore]
        private Image? _image {  get; set; } = null;

        [JsonIgnore]
        public int Width => _image?.Width ?? 0;

        [JsonIgnore]
        public int Height => _image?.Height ?? 0;

        public async Task<Image> GetImage()
        {
            if(_image == null)
            {
                if (Url == string.Empty)
                    throw new Exception("Url for image cannot be null!");

                var fileName = Path.Combine(LocalFileStorePath, Name + LocalFileFormat);
                if (File.Exists(fileName)) // Local cached copy
                {
                    _image = new Bitmap(fileName);
                }
                else if (Url.StartsWith("http://") || Url.StartsWith("https://")) // Web request
                {
                    var clientStream = await new HttpClient().GetByteArrayAsync(Url);
                    if (clientStream == null)
                        throw new Exception("Could not load image");
                    var download = new MemoryStream(clientStream);
                    _image = new Bitmap(download);
                    _image.Save(fileName, ImageFormat.Png);
                }
                else if (File.Exists(Url)) // Url is local
                {
                    // Verify the file is good before saving it in the cache
                    _image = new Bitmap(Url);
                    File.Copy(Url, fileName, true);
                }
                else
                    throw new Exception("Could not load image!");
            }

            return _image!;
        }

        public void Dispose()
        {
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
            }
        }
    }
}
