using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TSDB.Commands;
using TSDB.Commands.Slash;
using System.Security.Cryptography;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TSDB
{
    internal class Program
    {
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands {  get; set; }    
        private static jsonreader JsonReader { get; set; }
        static async Task Main(string[] args)
        {
            JsonReader = new jsonreader();
            await JsonReader.ReadJson();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = JsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };
            
            Client = new DiscordClient(discordConfig);
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });
            Client.Ready += Client_Ready;
            Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            Client.GuildMemberAdded += Client_GuildMemberAdded;
            Client.ModalSubmitted += Client_ModalSubmitted;

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { JsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            var slashCommandsConfig = Client.UseSlashCommands();
            Commands.RegisterCommands<TestCommands>();
            Commands.RegisterCommands<InteractionComponents>();
            slashCommandsConfig.RegisterCommands<BasicSL>();
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task Client_ModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
        {
            var values = e.Values;
            if (e.Interaction.Type == InteractionType.ModalSubmit && e.Interaction.Data.CustomId == "testModal")
            {
                var channel = await e.Interaction.Guild.CreateChannelAsync($"Заказ {e.Interaction.User.Username}'a", DSharpPlus.ChannelType.Text, e.Interaction.Channel.Parent);

                var random = new Random();
                
                var TicketEngine = new TicketEngine();

                int minvalue = 10000;
                int maxvalue = 99999;

                int randomNumber = random.Next(minvalue, maxvalue);
                string before_hash = e.Interaction.User.Username + e.Interaction.User.AvatarHash + randomNumber.ToString();

                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                byte[] bytes = Encoding.UTF8.GetBytes(before_hash);
                byte[] hash = md5.ComputeHash(bytes);
                string ticket_id = Convert.ToBase64String(hash);

                var ticket1 = new Ticket()
                {
                    username = e.Interaction.User.Username,
                    productName = values.Values.First(),
                    payMethod = values.Values.ToArray()[1],
                    loginMethod = e.Values.Values.ToArray()[2],
                    ticketNum = 0,
                    ticketId = ticket_id,
                };

                TicketEngine.StoreTicket(ticket1);
                
                var productEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.DarkButNotBlack,
                    Title = $"",
                    Description = $"Товар: {e.Values.Values.ToArray()[0]}"
                };

                var payMethodEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.DarkButNotBlack,
                    Title = $"",
                    Description = $"Способ оплаты: {e.Values.Values.ToArray()[1]}"

                };

                var productEmbed1 = new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.DarkButNotBlack)
                    .WithDescription($"Ticket ID: {ticket1.ticketId}")
                    .WithTitle($"{e.Interaction.User.Username} открыл запрос покупки"));
                productEmbed1.AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Danger, "closeButtonTicket", "Закрыть"),
                    new DiscordButtonComponent(ButtonStyle.Danger, "closeWithReasonButtonTicket", "Закрыть с причиной"),
                    new DiscordButtonComponent(ButtonStyle.Success, "openButtonTicket", "Назначить"),
                });


                var loginMethodEmbed = new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.DarkButNotBlack,
                    Title = $"",
                    Description = $"Cпособ выполнения заказа: {e.Values.Values.ToArray()[2]}"
                };
                await channel.SendMessageAsync(productEmbed1);
                await channel.SendMessageAsync(embed: productEmbed);
                await channel.SendMessageAsync(embed: payMethodEmbed);
                await channel.SendMessageAsync(embed: loginMethodEmbed);
            }
        }

        private static Task Client_GuildMemberAdded(DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs args)
        {
            throw new NotImplementedException();
        }

        private static async Task Client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            if (args.Interaction.Data.ComponentType == ComponentType.StringSelect)
            {
                switch (args.Id)
                {
                    case "items":
                        await args.Interaction.CreateResponseAsync(
                            InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().WithContent("Продукт выбран!").AsEphemeral(true)
                        );
                        await Task.Delay(1000);
                        await args.Interaction.DeleteOriginalResponseAsync();
                        break;
                    case "payments":
                        await args.Interaction.CreateResponseAsync(
                            InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().WithContent("Метод оплаты выбран!").AsEphemeral(true)
                        );
                        await Task.Delay(1000);
                        await args.Interaction.DeleteOriginalResponseAsync();
                        break;
                    case "joins":
                        await args.Interaction.CreateResponseAsync(
                            InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().WithContent("Способ входа выбран!").AsEphemeral(true)
                        );
                        await Task.Delay(1000);
                        await args.Interaction.DeleteOriginalResponseAsync();
                        break;
                }
            }
            if (args.Interaction.Data.ComponentType == ComponentType.Button)
            {
                switch (args.Interaction.Data.CustomId)
                {
                    case "ticket_cancel":
                        await args.Interaction.CreateResponseAsync(
                            InteractionResponseType.UpdateMessage,
                            new DiscordInteractionResponseBuilder().WithContent("Удачи!")
                        );
                        await Task.Delay(1000);
                        await args.Interaction.DeleteOriginalResponseAsync();
                        break;
                    case "ticket_create":
                        bool full = true;
                        foreach (...) // ВОТ ТУТ
                        {
                            if (component.CustomId == "items" || component.CustomId == "payments" || component.CustomId == "joins")
                            {
                                if (component.ToString() == null)
                                {
                                    full = false; break;
                                }
                            }
                        }
                        if (full)
                        {
                            var channel = await args.Interaction.Guild.CreateChannelAsync($"Тикет {args.Interaction.User.Username}'a", ChannelType.Text, args.Interaction.Channel.Parent);

                            var random = new Random();

                            var TicketEngine = new TicketEngine();

                            int minvalue = 10000;
                            int maxvalue = 99999;

                            int randomNumber = random.Next(minvalue, maxvalue);
                            string before_hash = args.Interaction.User.Username + args.Interaction.User.AvatarHash + randomNumber.ToString();

                            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
                            byte[] bytes = Encoding.UTF8.GetBytes(before_hash);
                            byte[] hash = md5.ComputeHash(bytes);
                            string ticket_id = Convert.ToBase64String(hash);

                            string item = "";
                            string payment = "";
                            string join = "";
                            foreach (var component in args.Interaction.Data.Components)
                            {
                                if (component.CustomId == "items")
                                {
                                    item = component.ToString();
                                }
                                else if (component.CustomId == "payments")
                                {
                                    payment = component.ToString();
                                }
                                else if (component.CustomId == "joins")
                                {
                                    join = component.ToString();
                                }
                            }

                            var ticket1 = new Ticket()
                            {
                                username = args.Interaction.User.Username,
                                productName = item,
                                payMethod = payment,
                                loginMethod = join,
                                ticketNum = 0,
                                ticketId = ticket_id
                            };

                            TicketEngine.StoreTicket(ticket1);

                            var productEmbed = new DiscordEmbedBuilder()
                            {
                                Color = DiscordColor.DarkButNotBlack,
                                Title = $"Тикет {args.Interaction.User.Username}'а"
                            };
                            productEmbed.AddField("Продукт", ticket1.productName, true);
                            productEmbed.AddField("Оплата", ticket1.payMethod, true);
                            productEmbed.AddField("Вход", ticket1.loginMethod, true);
                            productEmbed.AddField("ID тикета", ticket1.ticketId, true);

                            var productMessage = new DiscordMessageBuilder();
                            productMessage.AddComponents(new DiscordComponent[]
                            {
                                new DiscordButtonComponent(ButtonStyle.Danger, "closeButtonTicket", "Закрыть"),
                                new DiscordButtonComponent(ButtonStyle.Danger, "closeWithReasonButtonTicket", "Закрыть с причиной"),
                                new DiscordButtonComponent(ButtonStyle.Success, "openButtonTicket", "Назначить"),
                            });

                            await channel.SendMessageAsync(embed: productEmbed);
                        }
                        break;
                }
            }
            switch (args.Interaction.Data.CustomId)
            {
                case "closeButtonTicket":
                    var channelTicket = args.Interaction.Channel;
                    var channelInfo = args.Guild.GetChannel(1210941405640400946);
                    var messages = channelTicket.GetMessagesAsync().Result;

                    await channelTicket.DeleteAsync();
                    break;
                case "button3":
                    
                    break;
                case "modalButton":

                    var options_items = new List<DiscordSelectComponentOption>();
                    foreach (var item in JsonReader.items)
                    {
                        options_items.Add(new DiscordSelectComponentOption(
                                item.name,
                                item.id,
                                item.description,
                                isDefault: false,
                                emoji: new DiscordComponentEmoji(item.emoji)));
                    }
                    var dropdown_items = new DiscordSelectComponent("items", "Продукт", options_items, false, 1, 1);

                    var options_payments = new List<DiscordSelectComponentOption>();
                    foreach (var payment in JsonReader.payments)
                    {
                        options_payments.Add(new DiscordSelectComponentOption(
                                payment.name,
                                payment.id,
                                payment.description,
                                isDefault: false,
                                emoji: new DiscordComponentEmoji(payment.emoji)));
                    }
                    var dropdown_payments = new DiscordSelectComponent("payments", "Способ оплаты", options_payments, false, 1, 1);

                    var options_joins = new List<DiscordSelectComponentOption>();
                    foreach (var join in JsonReader.joins)
                    {
                        options_joins.Add(new DiscordSelectComponentOption(
                                join.name,
                                join.id,
                                join.description,
                                isDefault: false,
                                emoji: new DiscordComponentEmoji(join.emoji)));
                    }
                    var dropdown_joins = new DiscordSelectComponent("joins", "Вход в аккаунт", options_joins, false, 1, 1);

                    var btn_confirm = new DiscordButtonComponent(ButtonStyle.Primary, "ticket_create", "Подтвердить");
                    var btn_cancel = new DiscordButtonComponent(ButtonStyle.Danger, "ticket_cancel", "Отменить");


                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        .WithContent("123")
                        .AddComponents(dropdown_items)
                        .AddComponents(dropdown_payments)
                        .AddComponents(dropdown_joins)
                        .AddComponents(btn_confirm, btn_cancel)
                        .AsEphemeral(true));

                    break;
            }
        }

        private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
