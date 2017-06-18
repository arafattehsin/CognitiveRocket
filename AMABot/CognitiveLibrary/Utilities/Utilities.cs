using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace CognitiveLibrary.Utilities
{
    public class Utilities
    {
        public static byte[] GetImagesAsByteArrayFromUri(string contentUri)
        {
            var webClient = new WebClient();
            byte[] imageBytes = webClient.DownloadData(contentUri);
            return imageBytes;
        }
    }
}