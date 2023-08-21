# Bit Templates Playground

With [GitHub Codespaces](https://github.com/features/codespaces), you can access the ultimate online development platform, right in your browser - no downloads or installations required!

Experience lightning-fast speeds and seamless integration as you dive into the world of Bit BlazorUI, and elevate your web development game to new heights.

[![Open in Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/bitfoundation/bit-templates-playground/tree/develop)

**Instructions:**

1- Press `Ctrl + Shift + P` and run `watch sass`

2- Open new terminal in vscode and run:

`dotnet watch --project src/Server/Api/Bit.AdminPanel.Server.Api.csproj`

3- Open another terminal in vscode and run:

`dotnet watch --project src/Client/Web/Bit.AdminPanel.Client.Web.csproj`

4- Sign up using any email address you want

5- Open your database by double clicking on `src/Server/Api/Bit.AdminPanelDb.db`

7- To view your data, double click on `src/Server/Api/Bit.AdminPanelDb.db`

8- In your `Users` table, change `EmailConfirmed` from `0` to `1`

9- Sign in and explorer admin panel features!

Done!

Note: In order to view `confirmation email`, you can download it from `src/Server/Api/bin/Debug/net7.0/sent-emails`, then you can view it using any [eml viewer](https://msgeml.com/)