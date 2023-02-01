using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text;

namespace ZuneDiscordRPC {
    class DeezerAlbumArtProvider : IAlbumArtProvider {
        public string GetAlbumArt(string album, string artist) {
            string requestUrl = "http://api.deezer.com/search/album?q=artist:\"" + artist + "\" album:\"" + album + "\"";
            
            var request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.UserAgent = "ZuneDiscordRPC/<3.0> ( help@zunes.me )";
            request.Method = WebRequestMethods.Http.Get;

            var response = request.GetResponse().GetResponseStream();
            var reader = new StreamReader(response);
            string data = reader.ReadToEnd();

            //No Album Art Found ...
            if (data == "{\"data\":[],\"total\":0}") {
                return "modern_zune_logo";
            }

            int index = data.IndexOf("cover") + 8;
            data = data.Substring(index);
            index = data.IndexOf("\"");
            data = data.Substring(0, index);
            data = data.Replace("\\", "");

            return data;
        }
    }
}
