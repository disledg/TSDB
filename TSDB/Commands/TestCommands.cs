using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TSDB.Commands
{
    internal class TestCommands : BaseCommandModule
    {
        [Command("Test")]
        public async Task MyfirstCommand(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Hello");
        }


        [Command("dropdown-list")]
        public async Task MenuFirstVersion(CommandContext ctx)
        {
            try
            {
                var options = new List<DiscordSelectComponentOption>()
            {
                new DiscordSelectComponentOption(
                    "Имя кнопки без описания",
                    "option1"),

                new DiscordSelectComponentOption(
                    "Имя кнопки",
                    "label_with_desc",
                    "Вот оно описание"),

                new DiscordSelectComponentOption(
                    "Имя кнопки с эмоджи",
                    "label_with_desc_emoji",
                    "Вот оно описание",
                emoji: new DiscordComponentEmoji(854260064906117121)),

                new DiscordSelectComponentOption(
                    "Имя кнопки с эмоджи (Default)",
                    "label_with_desc_emoji_default",
                    "Вот оно описание")
            };
                var dropdown = new DiscordSelectComponent("dropdown", null, options, false, 1, 1);
                var dropdown1 = new DiscordSelectComponent("dropdown1", null, options, false, 1, 1);
                var message = new DiscordEmbedBuilder
                {
                    ImageUrl = "https://s2.mmommorpg.com/media/wide/scaled/genshin-wide.jpg.340x170_q75_crop-smart.jpg",
                    Color = DiscordColor.DarkButNotBlack ,
                };
                var messageBuilder = new DiscordMessageBuilder();
                messageBuilder.AddEmbed(message);
                messageBuilder.AddComponents(dropdown1);
                messageBuilder.AddComponents(dropdown);
                await ctx.Channel.SendMessageAsync(messageBuilder);

            }
            catch(Exception e) 
            { 
                Console.WriteLine(e);
            }
                

        }

        [Command("embed")]
        public async Task calc(CommandContext ctx)
        {
            var message = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Azure)
                .WithAuthor(ctx.User.Username)
                .WithDescription("Нахуй иди"));
            await ctx.Channel.SendMessageAsync(message);
        }
        [Command("modal")]
        public async Task Modal(CommandContext ctx)
        {
            var modalButton = new DiscordButtonComponent(ButtonStyle.Secondary, "modalButton", "Оформить заказ");
            var modalButton2 = new DiscordButtonComponent(ButtonStyle.Secondary, "modalButton2", "Обратиться в поддержку");

            var message = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.DarkButNotBlack)
                    .WithTitle("Обращение")
                    .WithDescription("Нажмите на кнопку ниже что бы создать обращение о покупке  или напиши свой ебучий гневный коммент нажав другую кнопку"));
                
            message.AddComponents(modalButton2);
            message.AddComponents(modalButton);

            await ctx.Channel.SendMessageAsync(message);
        }
    }
}
