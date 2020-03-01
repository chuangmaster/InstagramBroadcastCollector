using InstagramApiSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramBroadcastCollector
{
    interface IBroadcastProcess
    {
        Task<bool> DoAsync(string targetPageName);
    }
}
