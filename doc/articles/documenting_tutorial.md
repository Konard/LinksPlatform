# Documentation tutorial

## Table of Contents
* [Introduction](#introduction)
* [The First Steps](#the-first-steps)
* [Github](#github)
* [Documenting](#documenting)

## Introduction
> ⚠️ This tutorial is made for working from IDE, text editor. You can try another way: document from [github](https://github.com/) directly.

In this tutorial our team will try to plunge you into actions before reading theory if possible to prevent mood like
`I am reading these XML tags and cannot imagine how to use them`
You can skip any chapter if you think you have enough knowledge about it. Questions? Ask them on [out official Discord server](https://discord.gg/eEXJyjWv5e).

## The First Steps
1. See what documenting is
   1. Open any [repository with ready documentation](https://github.com/linksplatform/IO/blob/master/csharp/Platform.IO/TemporaryFile.cs)
   2. Look on the text after `\\\`. It is XML Documentation comments
   3. [See result](https://linksplatform.github.io/IO/csharp/api/Platform.IO.html)
2. Get your own channel in [Discord](https://discord.gg/eEXJyjWv5e)
   1. Join discord server
   2. Write in main/documenting chat "I want to document code"
3. Get your documentation task
   1. Choose any documentation task from to-do on [documentation projects board](https://github.com/orgs/linksplatform/projects/1)
   2. Say it in your own discord channel chat. Example: "I would like to take this task - `link`"

## GitHub
1. [Create an account](https://github.com)
2. Get invite to [linksplatform](https://github.com/linksplatform)
3. [Clone repository](https://docs.github.com/en/get-started/quickstart/fork-a-repo#cloning-your-forked-repository)
4. Create a branch.
   Use command `github branch`. Example: `github branch username_documenting`
5. Checkout the branch.
   Use command `github checkout`. Example: `github checkout username_documenting`

## Documenting
1. Write three forward-slashes(`///`) before method
   After that your IDE will create required tags for documentation. Now you see the draft that you will be working on. It is much better than reading XML tags and have no imagination about how you will use them
2. Read [Recommended XML tags for C# documentation](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)
   **Do not read all the page at once.** At first read about tags created by your IDE. Then you can sequentially read other tags that you will meet less often 
3. Kick out your perfectionism.
4. Kick out your fears.
5. Read the method body.
   If you do not understand something, try to get information from internet.
   Example: I see `Math.Pow` in the code and do not know what is it. I will search `Math.Pow` in my browser and will find [result](https://docs.microsoft.com/en-us/dotnet/api/system.math.floor?view=net-5.0) from `docs.microsoft.com`. Click on result link and you will see that this method `Returns the largest integral value less than or equal to the specified number.`
5. Write your first thoughts about what this method does in XML tags.
6. Save changes (`CTRL+S`)
7. Commit changes to your branch
   Use `git commit -m "MESSAGE"` command. Example `git commit -m "Update Life.AddHappiness() XML Docs"`
8. Push the commits to the remote repository.
   Use `git push` command.
9. Open the repository, open pull request TODO
