using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZuneDiscordRPC
{
    interface IAlbumArtProvider {
        string GetAlbumArt(string album, string artist);
    }
}
