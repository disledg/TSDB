using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSDB.Commands
{
    public class InteractionComponents : BaseCommandModule
    {
        [Command("Button")]
        public async Task Buttons(CommandContext ctx)
        {
            var button = new DiscordButtonComponent(ButtonStyle.Primary,"button1","Button");
            var button2 = new DiscordButtonComponent(ButtonStyle.Primary, "button2", "Button2");
            var message = new DiscordMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
                .WithTitle("Test"))
            .AddComponents(button,button2);
                
            await ctx.RespondAsync(message);
                
        }
        [Command("Help")]
        public async Task HelpCommand(CommandContext ctx)
        {
            var button3 = new DiscordButtonComponent(ButtonStyle.Primary, "button3", "Other commands help");
            var message = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithTitle("Help")
                    .WithColor(DiscordColor.Blurple))
                .AddComponents(button3);
            await ctx.RespondAsync(message);
        }
    }
}
