using System;
using System.Collections.Generic;
using System.Text;

namespace Tiktok_Downloader
{
    public class Information
    {
        public string author_id { get; set; }
        public string author_name { get; set; }
        public int comment_count { get; set; }
        public string create_time { get; set; }
        public string id { get; set; }
        public int like_count { get; set; }
        public int share_count { get; set; }
        public bool success { get; set; }
        public string token { get; set; }
    }
}
