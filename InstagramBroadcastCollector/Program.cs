using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstagramBroadcastCollector
{
    class Program
    {
        async static Task Main(string[] args)
        {
            Console.Write("Input your instagram UserName:");
            var userName = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Input your instagram Password:");
            var password = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Input your instagram target page name:");
            var targetPageName = Console.ReadLine();

            var InstaApi = await InitialInstaApi(userName, password);
            IBroadcastProcess process = new BroadcastProcess(InstaApi);
            await process.DoAsync(targetPageName);
            Console.WriteLine("Press any key to quit");
            Console.Read();
        }

        /// <summary>
        /// get initial API
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async static Task<IInstaApi> InitialInstaApi(string userName, string password)
        {
            var userSession = new UserSessionData()
            {
                UserName = userName,
                Password = password,
            };
            var delay = RequestDelay.FromSeconds(2, 2);

            var api = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.All))
                .SetRequestDelay(delay)
                .Build();
            if (!api.IsUserAuthenticated)
            {
                // login
                Console.WriteLine($"Logging in as {userSession.UserName}");
                delay.Disable();
                var logInResult = await api.LoginAsync();
                delay.Enable();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");
                }
            }
            return api;
        }
    }
}
