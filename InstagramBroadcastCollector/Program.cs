using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.SessionHandlers;
using System.IO;

namespace InstagramBroadcastCollector
{
    class Program
    {
        const string StateFile = "state.bin";

        async static Task Main(string[] args)
        {
            Console.Write("Input your instagram UserName:");
            var userName = Console.ReadLine();
            Console.WriteLine();
            Console.Write("Input your instagram Password:");
            string password = string.Empty;
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password = password.Substring(0, password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.KeyChar != '\u0000') // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
            } while (true);
            Console.WriteLine("\n\r");


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
                //.UseLogger(new DebugLogger(LogLevel.All))
                .SetRequestDelay(delay)
                // Session handler, set a file path to save/load your state/session data
                .SetSessionHandler(new FileSessionHandler() { FilePath = StateFile })
                .Build();
            //Load session
            try
            {
                if (File.Exists(StateFile))
                {
                    Console.WriteLine("Loading state from file");
                    using (var fs = File.OpenRead(StateFile))
                    {
                        api.LoadStateDataFromStream(fs);
                        // in .net core or uwp apps don't use LoadStateDataFromStream
                        // use this one:
                        // _instaApi.LoadStateDataFromString(new StreamReader(fs).ReadToEnd());
                        // you should pass json string as parameter to this function.
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (!api.IsUserAuthenticated)
            {
                // login
                Console.WriteLine($"Logging in as {userSession.UserName}");
                delay.Disable();
                var logInResult = await api.LoginAsync(false);
                delay.Enable();
                if (!logInResult.Succeeded)
                {
                    Console.WriteLine($"Unable to login: {logInResult.Info.Message}");

                    if (logInResult.Value == InstaLoginResult.ChallengeRequired)
                    {
                        var challenge = await api.GetChallengeRequireVerifyMethodAsync();
                        if (challenge.Succeeded)
                        {
                            if (challenge.Value.SubmitPhoneRequired)
                            {
                                Console.WriteLine("input your phone number:");
                                var phoneNumber = Console.ReadLine();
                                var submitPhone = await api.SubmitPhoneNumberForChallengeRequireAsync(phoneNumber);
                                if (submitPhone.Succeeded)
                                {
                                    var verifiedResult = await VerifiedCodeAsync(api);
                                    Console.WriteLine(verifiedResult ? "Login success!" : "Login fail!");
                                }
                                else
                                {
                                    throw new Exception("Submit phone number error...");
                                }
                            }
                            else
                            {
                                if (challenge.Value.StepData != null)
                                {
                                    Dictionary<int, string> verifyMethod = new Dictionary<int, string>();
                                    if (!string.IsNullOrEmpty(challenge.Value.StepData.PhoneNumber))
                                    {
                                        verifyMethod.Add(1, "phone");
                                    }
                                    if (!string.IsNullOrEmpty(challenge.Value.StepData.Email))
                                    {
                                        verifyMethod.Add(2, "email");
                                    }

                                    Console.WriteLine("Select one to get yor verification code:");
                                    foreach (var m in verifyMethod)
                                    {
                                        Console.WriteLine($"Press {m.Key} to use {m.Value}");
                                    }

                                    var key = Console.ReadLine();
                                    if (key == "1")
                                    {
                                        // send verification code to phone number
                                        var phoneNumber = await api.RequestVerifyCodeToSMSForChallengeRequireAsync();
                                        var verifiedResult = await VerifiedCodeAsync(api);
                                        Console.WriteLine(verifiedResult ? "Login success!" : "Login fail!");

                                    }
                                    else if (key == "2")
                                    {
                                        // send verification code to email
                                        var email = await api.RequestVerifyCodeToEmailForChallengeRequireAsync();
                                        var verifiedResult = await VerifiedCodeAsync(api);
                                        Console.WriteLine(verifiedResult ? "Login success!" : "Login fail!");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Challenge error");
                        }
                    }
                }
            }
            var state = api.GetStateDataAsStream();
            // in .net core or uwp apps don't use GetStateDataAsStream.
            // use this one:
            // var state = _instaApi.GetStateDataAsString();
            // this returns you session as json string.
            using (var fileStream = File.Create(StateFile))
            {
                state.Seek(0, SeekOrigin.Begin);
                state.CopyTo(fileStream);
            }
            return api;
        }

        /// <summary>
        /// verified code
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        private static async Task<bool> VerifiedCodeAsync(IInstaApi api)
        {
            Console.WriteLine("Input your verified code:");
            var code = Console.ReadLine();
            var verifyLogin = await api.VerifyCodeForChallengeRequireAsync(code);
            if (verifyLogin.Succeeded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

