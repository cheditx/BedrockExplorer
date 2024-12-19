# BedrockExplorer
This library is written completely in C#, allows you to query Minecraft Bedrock servers and receive information from them.

# Installation
```dotnet add package BedrockExplorer```

# Example

```csharp
namespace YourProject;

class YourClass {

    private static async Task Main() {

        var explorer = new BedrockExplorer();
        var query = await explorer.QueryServer(address: "zeqa.net", port: 19132, timeout: 5000);

        if (query.IsOk()) {
            var response = query.GetResponse();
            Console.WriteLine(response);
        } else {
            var error = query.GetError();
            Console.WriteLine(error);
        }
    }
}
```
