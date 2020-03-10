using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramBroadcastCollector.Models;
using Newtonsoft.Json;

namespace InstagramBroadcastCollector
{
    public class BroadcastProcess : IBroadcastProcess
    {
        private readonly IInstaApi _InstaApi;

        public BroadcastProcess(IInstaApi insta_InstaApi)
        {
            _InstaApi = insta_InstaApi;
        }
        public async Task<bool> DoAsync(string targetPageName)
        {
            try
            {
                // find live id 
                var broadcastId = await FindLivePageId(targetPageName);

                if (!string.IsNullOrEmpty(broadcastId))
                {
                    List<InstagramCommentDTO> data = new List<InstagramCommentDTO>();
                    // get comments
                    GetLiveComments(broadcastId, data);
                    ConsoleKey key;
                    do
                    {
                        Console.WriteLine("Press S to save current live comments, Q to quit.");
                        key = Console.ReadKey().Key;
                        Console.WriteLine();

                        if (key == ConsoleKey.S)
                        {
                            string json = JsonConvert.SerializeObject(data);
                            string filePath = await WriteJson(json);
                            Console.WriteLine($"your file is {filePath}");
                        }
                        else if (key == ConsoleKey.Q)
                        {
                            Console.WriteLine("quit the application.");
                            break;
                        }

                        await Task.Delay(1000);

                    } while (true);
                }
                else
                {
                    Console.WriteLine("There is no broadcast page on your Instagram account.");
                }

            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// find target page live id
        /// </summary>
        /// <param name="targetPageName"></param>
        /// <returns></returns>
        private async Task<string> FindLivePageId(string targetPageName)
        {
            var broadcastId = string.Empty;
            int times = 5;
            while (times-- > 0)
            {
                //get current live page
                var getBroadcastResult = await _InstaApi.LiveProcessor.GetSuggestedBroadcastsAsync();
                if (getBroadcastResult.Succeeded)
                {
                    var targetBroadcast = getBroadcastResult.Value.
                        FirstOrDefault(x => x.BroadcastOwner.UserName == targetPageName);
                    if (targetBroadcast != null)
                    {
                        broadcastId = targetBroadcast.Id;
                        break;
                    }
                }
                else
                    Console.WriteLine("Error while suggested broadcasts: " + getBroadcastResult.Info.Message);
                Thread.Sleep(5000);
            }
            return broadcastId;
        }

        /// <summary>
        /// get live comments
        /// </summary>
        /// <param name="broadcastId"></param>
        /// <returns></returns>
        private async Task GetLiveComments(string broadcastId, List<InstagramCommentDTO> data)
        {

            var gtm = new DateTime(1970, 1, 1);
            var now = DateTime.UtcNow;
            while (true)
            {
                var lastTS = Convert.ToInt32(((TimeSpan)now.Subtract(gtm)).TotalSeconds);
                var commentResult = await _InstaApi.LiveProcessor.GetCommentsAsync(broadcastId, $"{lastTS}", 4);
                if (commentResult.Succeeded)
                {
                    var comments = commentResult.Value.Comments;

                    for (int i = 0; i < comments.Count; i++)
                    {
                        Console.WriteLine($"[{comments[i].Pk}]{comments[i].User.UserName} said: {comments[i].Text}");
                        data.Add(new InstagramCommentDTO()
                        {
                            Pk = comments[i].Pk,
                            UserName = comments[i].User.UserName,
                            Text = comments[i].Text,
                            CreatedTime = comments[i].CreatedAtUtc
                        });

                        if (i + 1 == comments.Count)
                        {
                            now = comments[i].CreatedAtUtc;
                        }
                    }
                    await Task.Delay(2000);
                }
                else
                {
                    if (data.Count > 0)
                    {
                        Console.WriteLine("Broadcast has stopped.");
                        string json = JsonConvert.SerializeObject(data);
                        string filePath = await WriteJson(json);
                        Console.WriteLine($"your file is {filePath}");
                    }
                    else
                    {
                        Console.WriteLine("Broadcast maybe was not existed.");
                    }
                    break;
                }
            }
        }

        private async Task<string> WriteJson(string jsonContent)
        {
            var filePath = $"Comments{DateTime.Now.ToString("yyyyMMddhhmmss")}.json";
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write(jsonContent);
            }
            return filePath;
        }
    }
}
