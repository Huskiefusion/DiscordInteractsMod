using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.Chat;
using Discord.WebSocket;
using Terraria.Net;
using Terraria.Localization;
using Discord.Commands;
using Discord;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;
using System.IO;

namespace DiscordInteractsMod
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.

    enum PacketActionType { Kill = 'k' };
	public class DiscordInteractsMod : Mod
	{
		static internal ChatConfig ChatConfig;
		string apiKey;
        string defaultTextChannel;
		DiscordSocketClient client;
		//CommandService commandService;
		readonly Random rand = new();

		readonly string[] discordKillMessages = [
			"{sender} has deemed {victim}'s death... necessary.",
			"{sender} decided that {victim} is bad for business.",
			"Dear {victim}, I hope your day fucking sucks! - With Love, {sender}",
			"{sender} has decided to make Cream of {victim} soup. Delicious!",
			"{victim} got sent to Brazil by {sender}.",
			"{sender} gave {victim} a terminal case of Ligma." ];
		public override void Load()
		{
			//eeeee
			apiKey = ChatConfig.apiKey;
			Console.WriteLine("Started!");
            var conf = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.All
            };
            client = new DiscordSocketClient(conf);
			try { client.LoginAsync(TokenType.Bot, apiKey); }
			catch (Exception ex) { Logger.Warn("Failed to detect a valid API key. If this is intentional, this can be ignored.\n Expection: "+ex ); base.Load(); return; }
			client.StartAsync();


			client.MessageReceived += HandleMessageRecieved;
			Logger.Info("Mod Loaded!");
			base.Load();
		}

        public override void Unload()
        {
			client.StopAsync();
            base.Unload();
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
			PacketActionType packetType = (PacketActionType) reader.ReadByte();
			switch (packetType)
			{
                case PacketActionType.Kill:
                        Main.player[reader.ReadInt32()].KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(reader.ReadString()), 999, 1);
					break;
                default:
					break;
			}
        }

        Task HandleMessageRecieved(SocketMessage message)
		{
            List<Player> players = [.. Main.ActivePlayers];
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return Task.CompletedTask;
            }

            if ((message as SocketUserMessage) == null) { return Task.CompletedTask; }
            if (message.Author.IsBot) { return Task.CompletedTask; }

            //bool isDev = message.Author.GlobalName == "Huskiefusion";
            //bool shouldConfirm = message.Content.Contains("confirm");

            Logger.Info($"{message.Author.GlobalName} sent message: {message.CleanContent}");
            if (ChatConfig.sendMessagesInIngameChat)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{message.Author.GlobalName}> {message.CleanContent}"),
                    Microsoft.Xna.Framework.Color.Lavender);
            }
            // Get list of active players
            if (message.Content.StartsWith("!whuwu"))
            {
                List<string> active = [];
                foreach (var player in Main.ActivePlayers)
                {
                    active.Add($"\"{player.name}\"");
                };
                message.Channel.SendMessageAsync($"Active Players: {string.Join(", ", [.. active])}");
            }

            if (message.Content.StartsWith("!owo"))
            {
                foreach (var player in Main.ActivePlayers)
                {
                    if (message.Author.GlobalName == "Huskiefusion" && message.Content.Contains("confirm"))
                        message.Channel.SendMessageAsync("owoing as we speak");

                    if (message.Content.Contains(player.name))
                    {
                        // get random reason
                        var reason = discordKillMessages[rand.Next(discordKillMessages.Length)];
                        // format reason
                        reason = reason.Replace("{sender}", message.Author.GlobalName);
                        reason = reason.Replace("{victim}", player.name);
                        // Kill yourself, NOW!! >:3
                        player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(reason), 999f, 1);
                        if (Main.netMode == NetmodeID.SinglePlayer)
                        {
                            break;
                        }
                        var packet = GetPacket();
                        packet.Write((byte)PacketActionType.Kill);
                        packet.Write(player.whoAmI);
                        packet.Write(reason);
                        packet.Send();

                        message.Channel.SendMessageAsync($"<@{message.Author.Id}>, you monster!");
                    }
                };
            }

            if (message.Author.GlobalName == "Huskiefusion" && message.Content.Contains("confirm"))
                message.Channel.SendMessageAsync("Sending Message!");
            return Task.CompletedTask;
        }
	}
}
