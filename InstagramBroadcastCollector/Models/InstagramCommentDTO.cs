using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramBroadcastCollector.Models
{
    public class InstagramCommentDTO
    {
        public long Pk { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
