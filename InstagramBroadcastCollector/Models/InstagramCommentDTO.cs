using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InstagramBroadcastCollector.Models
{
    public class InstagramCommentDTO
    {
        public long Pk { get; set; }
        [JsonProperty("name")]
        public string UserName { get; set; }
        [JsonProperty("comment")]
        public string Text { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
