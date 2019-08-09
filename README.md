# multi-turn prompt (with sync id)

Sample copied from https://github.com/Microsoft/botbuilder-samples.git and modified to include a synchronization id on messages sent to and from the bot.

Users sometimes send multiple messages in quick succession. If the bot has already processed and replied to a specific message, successive messages intended as reponses to the same bot message should be ignored.

This is an interesting problem.  No doubt there are different ways to solve it.  One method for WebChat/DirectLine could be to track the last message sent from the bot using a generated id.  On the WebChat side, ensure messages sent to the bot include that same id.  When the bot receives messages, ensure only one per conversation is processed at a time. And finally, ignore those which do not contain the expected id.

This bot has been created using [Bot Framework](https://dev.botframework.com), it shows how to use the prompts classes included in `botbuilder-dialogs`.  This bot will ask for the user's name and age, then store the responses. It demonstrates a multi-turn dialog flow using a text prompt, a number prompt, and state accessors to store and retrieve values.

## Prerequisites

- [Bot Channels Registration](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-quickstart-registration)
	This is required in order to obtain a MicrosoftAppId and MicrosoftAppPassword, as well as a DirectLineSecret, for appsettings.json

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 2.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## To try this sample

- Clone the repository

    ```bash
    git clone https://github.com/EricDahlvang/MultiTurnPromptBot-SyncId.git
    ```
- Add required settings to appsettings.json
	MicrosoftAppId
	MicrosoftAppPassword
	DirectLineSecret

- In a terminal, navigate to `MultiTurnPromptBot-SyncId`

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/csharp_dotnetcore/05.multi-turn-prompt` folder
  - Select `MultiTurnPromptBot.csproj` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the Bot Framework Emulator version 4.3.0 or greater from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

## Prompts

A conversation between a bot and a user often involves asking (prompting) the user for information, parsing the user's response,
and then acting on that information. This sample demonstrates how to prompt users for information using the different prompt types
included in the [botbuilder-dialogs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-dialog?view=azure-bot-service-4.0) library
and supported by the SDK.

The `botbuilder-dialogs` library includes a variety of pre-built prompt classes, including text, number, and datetime types. This
sample demonstrates using a text prompt to collect the user's name, then using a number prompt to collect an age.

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Dialogs](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-dialog?view=azure-bot-service-4.0)
- [Gathering Input Using Prompts](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-prompts?view=azure-bot-service-4.0&tabs=csharp)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
