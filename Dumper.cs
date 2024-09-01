using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace LambCustomModel
{
	public class Dumper : BaseMonoBehaviour
	{
		public static Dumper Instance;

		private void Start()
		{
			if (Dumper.Instance != null)
			{
				GameObject.Destroy(this.gameObject);
			}
			else
			{
				Dumper.Instance = this;
				GameObject.DontDestroyOnLoad(this.gameObject);
			}
		}

		private void Update()
		{
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.D))
			{
				this.DumpSceneToFile();
			}
		}

		private void DumpSceneToFile()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

			GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			foreach (GameObject root in roots)
			{
				LogTransformAndChildren(ref stringBuilder, root.name, root.transform);

				stringBuilder.AppendLine("");
			}

			Debug.LogWarning("Dumping in " + Application.persistentDataPath);
			System.IO.File.WriteAllText(Application.persistentDataPath.Replace("\\", "/") + "/Dump.txt", stringBuilder.ToString());
		}

		private void LogTransformAndChildren(ref System.Text.StringBuilder stringBuilder, string path, Transform transform)
		{
			Component[] components = transform.GetComponents<Component>();
			string componentsLog = ": ";
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] == null)
				{
					componentsLog += "NULL";
					continue;
				}

				if (i > 0)
				{
					componentsLog += ", " + components[i].GetType();
				}
				else
				{
					componentsLog += components[i].GetType();
				}
			}

			stringBuilder.AppendLine(path + componentsLog);

			foreach (Transform child in transform)
			{
				this.LogTransformAndChildren(ref stringBuilder, path + "/" + child.name, child);
			}
		}
	}
}
