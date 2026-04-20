using UnityEngine;
using UnityEngine.Networking;
using HarmonyLib;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;

namespace Sparkipelago {
	public enum MusicType {
		VANILLA = 0,
		STAGE = 1,
		LOAD = 2,
		LOOP = 3
	}
	
	class MusicRandomization {
		[HarmonyPatch(typeof(StageMusicControl), "FixedUpdate")]
		private class StageMusicPatch {
			private static void Prefix(StageMusicControl __instance, int ___frame, ref bool ___Started) {
				if (Sparkipelago.musicRando != (int)MusicType.VANILLA) {
					if (___frame != __instance.FrameToStartMusicAt) return;
					string musicPath = Path.Combine(Application.dataPath, "../apmusic");
					DirectoryInfo dir = new DirectoryInfo(musicPath);
					FileInfo[] info = dir.GetFiles("*.ogg");
					
					if (info.Length == 0) {
						Sparkipelago.musicRando = (int)MusicType.VANILLA;
						return;
					}
					___Started = true;
					
					System.Random rnd;
					if (Sparkipelago.musicRando == (int)MusicType.STAGE) {
						rnd = new System.Random(Sparkipelago.musicSeed);
						for (int i = 0; i < Save.CurrentStageIndex; i++) {
							rnd.Next();
						}
					} else {
						rnd = new System.Random();
					}
					
					loadMusic(__instance.MainSource, info[rnd.Next(info.Length)].FullName);
				}
			}
			
			private static void Postfix(StageMusicControl __instance, bool ___Started) {
				if (Sparkipelago.musicRando == (int)MusicType.LOOP) __instance.MainSource.loop = false;
				
				if (!__instance.MainSource.isPlaying && ___Started && Sparkipelago.musicRando == (int)MusicType.LOOP && !loadingMusic) {
					string musicPath = Path.Combine(Application.dataPath, "../apmusic");
					DirectoryInfo dir = new DirectoryInfo(musicPath);
					FileInfo[] info = dir.GetFiles("*.ogg");
					var rnd = new System.Random();
					loadMusic(__instance.MainSource, info[rnd.Next(info.Length)].FullName);
				}
			}
		}
		
		static Dictionary<string, AudioClip> music;
		static bool loadingMusic;
		
		static Task<AudioClip> loadMusic(AudioSource source, string path) {
			loadingMusic = true;
			return Task.Run(() => {
				string uri = "file://" + path;
				MelonLogger.Msg("Randomizing Music: {0}", uri);
				if (!music.ContainsKey(path)) {
					using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS)) {
						www.SendWebRequest();
						while (!www.isDone);
						music.Add(path, DownloadHandlerAudioClip.GetContent(www));
					}
				}
				
				source.clip = music[path];
				source.clip.LoadAudioData();
				source.Play();
				loadingMusic = false;
				return music[path];
			});
		}
		
		public static void registerMusic() {
			music = new Dictionary<string, AudioClip>();
		//	ThreadStart threadFn = new ThreadStart(registerMusicThreaded);
		//	Thread thread = new Thread(threadFn);
		//	thread.Start();
		//	registerMusicThreaded();
		}
	}
}
