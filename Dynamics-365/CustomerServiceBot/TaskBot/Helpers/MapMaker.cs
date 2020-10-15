using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TaskBot.Helpers
{
    public class MapMaker
    {
        public static string Process(string incomingImageUrl, string sid)
        {
            var imageLocation = SaveImageLocally(incomingImageUrl, sid);
            return imageLocation;
        }

        private static string SaveImageLocally(string imageUrl, string sid)
        {
            var root = "/wwwroot";
            var dir = "/images/";
            var filename = $"{sid}.jpg";
            var path = Environment.CurrentDirectory + root + dir;
            var saveLocation = path + filename;
            var rootLocation = dir + filename;

            if (!Directory.Exists(path))
            {
                var di = Directory.CreateDirectory(path);
            }

            using (var httpClient = new HttpClient())
            {
                byte[] imageBytes = httpClient
                        .GetByteArrayAsync(imageUrl).Result;
                FileStream fs = new FileStream(saveLocation, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(imageBytes);
            }

            return rootLocation;
        }

        public static string GetAddressImage(string customerAddress)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    string addressUrl = $"http://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{customerAddress}/16?key=At-9uaLoghwXOeWXbc1bmz4XLcczj0hzL7YeBweBRr0Zx8BFIO9cl6XCU_Jj4sCw";
                    var webClient = new WebClient();
                    byte[] imageBytes = webClient.DownloadData(addressUrl);
                    string url = "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                    return url;
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}
