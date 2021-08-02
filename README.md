# Keydrop Sniffer
The application tracks whether a new code has appeared and tests the caught codes using the Key Drop API.

> :warning: **DISCLAIMER**: This is an unofficial application that is not affiliated with the KeyDrop.PL website!

> :information_source: **IMPORTANT**: You must be a member [of this Discord](https://discord.gg/8speHJbbCR) for the app to download the codes!

## Installation
If you want to install app, firstly you must build solution. Check **Build** section below!

- Build soluction
- Open .exe file
- Provide Discord Token and KeyDrop cookies (before you get cookies, you must be logged in)
- To make sure, your KeyDrop cookies are correct, check gold/wallet ammount at the bottom
- Fill form and wait for codes! :tada:

I recommend use followed settings:

![settings_image](https://i.imgur.com/mY5hBqj.png)

Remember that Discord has its own global Rate Limit, which is 50 queries per second. Exceeding the given limit will result in the following response: `429 Too Many Requests` (1 cycle = 1 request)

## Build
To build solution you must have Visual Studio with C# extension

- Open `.cproj` file with Visual Studio
- Make sure you have `Debug` selected at the top of screen
- Go to `Build` tab
- Click `Build solution` option and wait
- Go to project location then `bin/Debug/net5.0-windows10.0.17763.0`
- You can run application using .exe file

## Where can i find KeyDrop cookies?
Obtaining cookies from the KeyDrop site is simple, but it can be problematic for people who have never delved into the KeyDrop site. Below is an instruction to help you get cookies.

- Go to [KeyDrop](https://key-drop.com) site
- Login with your Steam account
- After login, go to homepage
- Press `F12` then open `Network` tab
- Refresh page
- Find `pl/` or `en/` record (name depend of your language) and click with it
- On the right side, you will see another window with record details. Go to `Header` tab, and check `Request Header` section
- Find and copy value of `cookie`
- Paste it to the app
- Done!
