using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Windows;

namespace VideoClipEditor
{
    class GfycatService
    {

        public static void UploadVideo(string file)
        {

            var client = new RestClient("https://api.gfycat.com/v1/");

            var request = new RestRequest("gfycats", Method.POST);
            client.ExecuteAsync<UploadDetails>(request, response => ReceivedDetails(response, file));
            //IRestResponse response = client.ExecuteAsync(request);
            //var content = response.Content;

        }

        private static void ReceivedDetails(IRestResponse<UploadDetails> response, string file)
        {
            Console.WriteLine(response.Data.Gfyname);

            var client = new RestClient("https://filedrop.gfycat.com");

            var request = new RestRequest("", Method.POST);

            request.AddParameter("key", response.Data.Gfyname);
            
            request.AddFile("file", file, "video/webm");
            

            client.ExecuteAsync(request, r =>
            {
                Console.WriteLine(r.Content);
                string url = "https://gfycat.com/" + response.Data.Gfyname;
                try
                {
                    System.Diagnostics.Process.Start(url);
                }
                catch(System.Exception e)
                {
                    MessageBox.Show("Gfycat can be found at " + url);
                }

            });
            
        }

        private class UploadDetails
        {
            public bool IsOK { get; set; }
            public string Gfyname { get; set; }
            public string Secret { get; set; }
            public string UploadType { get; set; }
        }
    }

    
}
