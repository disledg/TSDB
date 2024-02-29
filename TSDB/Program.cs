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
            if (args.Id == "dropdown" && args.Interaction.Data.ComponentType == ComponentType.StringSelect)
            {
                var options = args.Values;
                foreach (var option in options)
                {
                    switch (option)
                    {
                        case "option1":
                            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"Выбрана опция: {args.Values.ToString()}")
                .AsEphemeral(true)); // Ответ будет виден только автору
                            break;

                        case "option2":
                            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"{args.User.Username} has selected Option 2"));
                            break;

                        case "option3":
                            await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent($"{args.User.Username} has selected Option 3"));
                            break;
                        
                    }
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
                    /*var modal = new DiscordInteractionResponseBuilder()
                        .WithTitle("Test Modal")
                        .WithCustomId("testModal")
                        .AddComponents(new TextInputComponent("Укажите интересующий товар из ассортимента", "productname", "Nitro Full 1 месяц...", null, true, TextInputStyle.Short))
                        .AddComponents(new TextInputComponent("Укажите способ оплаты", "paymentmethod", "Сбербанк... Тинькофф...", null, true, TextInputStyle.Short))
                        .AddComponents(new TextInputComponent("Выберите способ выполнения заказа", "loginmethod", "Вход по QR-code... Вход по логину и паролю...", null, true, TextInputStyle.Short));
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);*/

                    var options = new List<DiscordSelectComponentOption>();
                    foreach (TicketItem item in JsonReader.items)
                    {
                        options.Append(new DiscordSelectComponentOption(
                            item.name,
                            item.id,
                            item.description));

                        Console.WriteLine($"{item.name} {item.id} {item.description}");
                    }

                    
                    
                    var dropdown_items = new DiscordSelectComponent("items", null, options, false, 1, 1);
                    /*var message = new DiscordEmbedBuilder
                    {
                        ImageUrl = "https://s2.mmommorpg.com/media/wide/scaled/genshin-wide.jpg.340x170_q75_crop-smart.jpg",
                        Color = DiscordColor.DarkButNotBlack,
                    };*/
                    var messageBuilder = new DiscordMessageBuilder();
                    messageBuilder.AddComponents(dropdown_items);


                    await args.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Ты нажал на кнопку!")
                            .AsEphemeral(true)
                            .AddComponents(dropdown_items)
                    );

                    break;
                


            }
        }

        private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
