using AutoDocs;
using Discord.Webhook;

string filePath = args[0];
bool hasWebhook = args.Length > 1;

string convertedMarkdown = new MarkdownConverter().Convert(File.ReadAllText(filePath));

Console.WriteLine(convertedMarkdown);

if (hasWebhook)
{
    string webhookUrl = args[1];
    DiscordWebhookClient webhook = new(webhookUrl);
    await webhook.SendMessageAsync(convertedMarkdown);
}

Console.WriteLine("Done!");