using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace LambCustomModel
{
	[BepInPlugin("org.monamiral.plugins.lambcustommodel", "Lamb Custom Model", "1.0.0.0")]
	[BepInProcess("Cult Of The Lamb.exe")]
	public class Plugin : BaseUnityPlugin
	{
		public static Harmony _harmony;

		private void Awake()
		{
			_harmony = new Harmony("org.monamiral.plugins.lambcustommodel");
			_harmony.PatchAll();
		}

		// Replace the texture when initializing skeleton animation.
		[HarmonyPatch(typeof(Spine.Unity.SkeletonAnimation))]
		internal class SkeletonAnimationPatch
		{
			private static UnityWebRequest request;
			public static Texture2D texture;

			[HarmonyPatch("Initialize")]
			[HarmonyPostfix]
			public static void InitializePatch(ref Spine.Unity.SkeletonAnimation __instance)
			{
				Renderer renderer = __instance.GetComponent<Renderer>();
				if (renderer == null || renderer.sharedMaterial == null)
				{
					return;
				}

				Debug.LogWarning("Replacing texture on " + renderer);
				__instance.StartCoroutine(ReplaceTexture(renderer));
			}

			private static IEnumerator ReplaceTexture(Renderer renderer)
			{
				if (renderer.sharedMaterial.mainTexture.name != "player-main")
				{
					yield break;
				}

				// Wait for someone else to finish loading the texture.
				while (request != null)
				{
					yield return null;
				}

				if (texture == null)
				{
					Debug.LogWarning("Loading texture.");

					using (request = UnityWebRequestTexture.GetTexture(Application.dataPath + "/../BepInEx/plugins/LambCustomModel/player-main.png"))
					{
						yield return request.SendWebRequest();

						if (request.result != UnityWebRequest.Result.Success)
						{
							Debug.LogError(request.error);
						}
						else
						{
							Debug.Log("Texture found.");
							texture = DownloadHandlerTexture.GetContent(request);
							texture.name = "player-main";
						}

						request = null;
					}
				}

				renderer.sharedMaterial.mainTexture = texture;
			}
		}

		// Add Shift+R to reload the file for easier edition.
		[HarmonyPatch(typeof(PlayerAbility))]
		internal class PlayerAbilityPatch
		{
			[HarmonyPatch("Update")]
			[HarmonyPostfix]
			public static void UpdatePatch(ref PlayerAbility __instance)
			{
				if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
				{
					Debug.LogWarning("Reloading textures");

					SkeletonAnimationPatch.texture = null;

					foreach (Spine.Unity.SkeletonAnimation animation in __instance.GetComponentsInChildren<Spine.Unity.SkeletonAnimation>())
					{
						animation.Initialize(false);
					}
				}
			}
		}
	}
}