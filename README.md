# InstagramBroadcastCollector
Use Instagram private api to get live comments, this project implemented InstagramApiSharp to get comments.<br>
you can get this library by Nuget [![NuGet](https://img.shields.io/nuget/v/InstagramApiSharp.svg)](https://www.nuget.org/packages/InstagramApiSharp)<br>

# How to use?
1. Input your Instagram account user name
2. Input your account password
3. Input your target page name that you have alread followed.
4. The application will collect all comments and write into a json file in same directory with execute file(.exe) after target page living.

# Note
Application saves a bin file after login. This file lets application doesn't need to send a login request at next time. The file saved vailid session in local storage. If you want to change account, you need to delete the file named state.bin.<br>
If login result is ChallengeRequired, you will be ask choose a way to get verification code. You will login after input your verification from your source that you chose.<br>

# Version
2020031601
* [Add] Add two factor validation.

2020031001
* [Update] Make application press button to save json file when page is liveing.

2020030502
* [Update] Make application to check target page live status 5 times in 25 seconds.

2020030501
* [Update] Hidie password when user are tying. 



# Thanks
[ramtinak](https://github.com/ramtinak/InstagramApiSharp) for contribution

# License
MIT.
