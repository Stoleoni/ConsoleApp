using System;
using PlayerApp.Constants;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerApp
{
    internal class HttpClientRequestHandler : IRequestHandler
    {
        public string GetPlayers(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();


                var response = client.GetStringAsync(new Uri(url)).Result;

                return response;
            }
        }
    }
}
