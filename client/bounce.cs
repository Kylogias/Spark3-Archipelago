using MelonLoader;
using HarmonyLib;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using System.Text;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Sparkipelago {
	class Bounce {
		public static DeathLinkService deathLink;

		public static void onDisconnect() {
			if (deathLink != null) {
				deathLink.DisableDeathLink();
				deathLink = null;
			}
		}

		public static void onConnect() {
			deathLink = DeathLinkProvider.CreateDeathLinkService(Sparkipelago.currentSession);
			deathLink.OnDeathLinkReceived += HandleDeathLink;
			Sparkipelago.currentSession.Socket.PacketReceived += HandlePacket;
			updateTags();
		}

		public static void updateTags() {
			List<string> tags = new List<string>();
			if (APSave.file.client.deathLink) tags.Add("DeathLink");
			if (APSave.file.client.trapLink) tags.Add("TrapLink");
			Sparkipelago.currentSession.ConnectionInfo.UpdateConnectionOptions(tags.ToArray());
		}

		public static void trySendTrap(ItemIds trap) {
			string trapName = Traps.trapIdToName(trap);
			if (trapName == "") return;
			if (APSave.file.client.trapLink && Sparkipelago.currentSession != null) {
				Sparkipelago.currentSession.Socket.SendPacketAsync(new BouncePacket {
					Tags = new List<string>{"TrapLink"},
					Data = new Dictionary<string, JToken>{
						{"time", DateTime.UtcNow.ToUnixTimeStamp()},
						{"source", APSave.getAPConnect().slot},
						{"trap_name", trapName}
					}
				});
			}
		}
		
		static Dictionary<string, ItemIds> TRAP_TO_ITEM = new Dictionary<string, ItemIds>{
			{"Nightmare Trap", ItemIds.NIGHTMARE_TRAP},
			{"Confuse Trap", ItemIds.NIGHTMARE_TRAP},
			{"Confusion Trap", ItemIds.NIGHTMARE_TRAP},
			{"Fear Trap", ItemIds.NIGHTMARE_TRAP},
			{"Laser Trap", ItemIds.LASER_TRAP},
			{"Spike Ball Trap", ItemIds.LASER_TRAP},
			{"Flint Trap", ItemIds.FLINT_TRAP},
			{"Ninja Trap", ItemIds.FLINT_TRAP},
			{"Police Trap", ItemIds.FLINT_TRAP},
			{"Army Trap", ItemIds.FLINT_TRAP},
			{"Spring Trap", ItemIds.SPRING_TRAP},
			{"Push Trap", ItemIds.SPRING_TRAP},
			{"Eject Ability", ItemIds.SPRING_TRAP},
			{"Gravity Trap", ItemIds.GRAVITY_TRAP},
			{"Honey Trap", ItemIds.GRAVITY_TRAP},
			{"Sticky Floor Trap", ItemIds.GRAVITY_TRAP},
			{"Iron Boots Trap", ItemIds.GRAVITY_TRAP},
			{"Disable A Trap", ItemIds.GRAVITY_TRAP},
			{"Zoom Trap", ItemIds.ZOOM_TRAP},
			{"Zoom In Trap", ItemIds.ZOOM_TRAP},
			{"Bald Trap", ItemIds.BALD_TRAP},
			{"Invisible Trap", ItemIds.BALD_TRAP},
			{"Invisibility Trap", ItemIds.BALD_TRAP},
			{"Invisiball Trap", ItemIds.BALD_TRAP},
			{"Damage Trap", ItemIds.DAMAGE_TRAP},
			{"Bomb Trap", ItemIds.DAMAGE_TRAP},
			{"Bomb", ItemIds.DAMAGE_TRAP},
			{"Bonk Trap", ItemIds.DAMAGE_TRAP},
			{"TNT Trap", ItemIds.DAMAGE_TRAP},
			{"TNT Barrel Trap", ItemIds.DAMAGE_TRAP},
			{"Reverse Trap", ItemIds.REVERSE_TRAP},
			{"Reversal Trap", ItemIds.REVERSE_TRAP},
			{"Reverse Controls Trap", ItemIds.REVERSE_TRAP},
			{"Control Ball Trap", ItemIds.REVERSE_TRAP},
			{"Slow Trap", ItemIds.SLOW_TRAP},
			{"Slowness Trap", ItemIds.SLOW_TRAP},
			{"Fast Trap", ItemIds.FAST_TRAP}
		};
		
		private static void HandlePacket(ArchipelagoPacketBase packet) {
			switch (packet.PacketType) {
				case ArchipelagoPacketType.Bounced:
					BouncedPacket bounced = (BouncedPacket)packet;
					if (bounced.Tags.Contains("TrapLink") && APSave.file.client.trapLink) {
						if (bounced.Data.ContainsKey("trap_name")) {
							StringBuilder sb = new StringBuilder("", 65536);
							sb.Append(string.Format("<color=#E08020>Trap Link: Received {0}", (string)bounced.Data["trap_name"]));
							if (bounced.Data.ContainsKey("source")) {
								sb.Append(string.Format(" From {0}", bounced.Data["source"]));
							}
							sb.Append("</color>");

							MelonLogger.Msg(bounced.Data["trap_name"].ToString());
							if (TRAP_TO_ITEM.ContainsKey((string)bounced.Data["trap_name"])) {
								ItemIds trapItem = TRAP_TO_ITEM[(string)bounced.Data["trap_name"]];
								sb.Append(string.Format(" (converted to {0})", Traps.trapIdToName(trapItem)));
								Traps.prioItemQueue.Enqueue(trapItem);
								Sparkipelago.messages.Enqueue(sb.ToString());
							}
						}
					}
					break;
			}
		}

		static bool isDeathLink;
		public static void HandleDeathLink(DeathLink deathLink) {
			if (Sparkipelago.player) {
				StringBuilder sb = new StringBuilder("", 65536);
				sb.Append("<color=#E02010>Death Link: ");
				if (deathLink.Cause != null) sb.Append(deathLink.Cause);
				else sb.Append(deathLink.Source);
				sb.Append("</color>");
				Sparkipelago.messages.Enqueue(sb.ToString());
				
				PlayerHealthAndStats.PlayerHP = -1;
				isDeathLink = true;
				Sparkipelago.player.GetComponent<HurtControl>().CheckForDeathAndKill();
			}
		}

		[HarmonyPatch(typeof(HurtControl), "PlayedDied")]
		private static class OnDeathPatch {
			private static void Prefix() {
				if (APSave.file.client.deathLink && !isDeathLink) {
					deathLink.SendDeathLink(new DeathLink(APSave.getAPConnect().slot));
				}
				isDeathLink = false;
			}
		}
	}
}