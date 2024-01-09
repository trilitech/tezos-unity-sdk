using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace TezosSDK.Editor.Scripts
{

	public class IssueReportFormWindow : EditorWindow
	{
		private readonly string[] issueTypes =
		{
			"Select a issue type...", "Fix", "Critical Bug", "Bug", "Feature", "Other"
		};
		private string additionalContext = "";
		private string email = "";
		private string errorMessage = "";
		private string issueDefinition = "";
		private int issueTypeIndex;
		private string sdkVersion = "";
		private string stepsToReproduce = "";
		private string successMessage = "";
		private string unityVersion = "";

		private void OnGUI()
		{
			GUILayout.Label("Issue Report Form", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Issue Definition*", GUILayout.Width(100));
			issueDefinition = EditorGUILayout.TextArea(issueDefinition, GUILayout.Height(60));
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Steps To Reproduce (Optional)", GUILayout.Width(200));
			stepsToReproduce = EditorGUILayout.TextArea(stepsToReproduce, GUILayout.Height(60));
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Additional Context (Optional)", GUILayout.Width(200));
			additionalContext = EditorGUILayout.TextArea(additionalContext, GUILayout.Height(60));
			EditorGUILayout.Space();

			if (string.IsNullOrEmpty(unityVersion))
			{
				unityVersion = Application.unityVersion;
			}

			unityVersion = EditorGUILayout.TextField("Unity Version*", unityVersion);
			EditorGUILayout.Space();

			if (string.IsNullOrEmpty(sdkVersion))
			{
				sdkVersion = GetSDKVersion();
			}

			sdkVersion = EditorGUILayout.TextField("SDK Version*", sdkVersion);
			EditorGUILayout.Space();

			email = EditorGUILayout.TextField("Email (Optional)", email);
			EditorGUILayout.Space();

			issueTypeIndex = EditorGUILayout.Popup("Issue Type*", issueTypeIndex, issueTypes);
			EditorGUILayout.Space();

			ValidateFields();

			if (!string.IsNullOrEmpty(errorMessage))
			{
				EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
			}

			if (!string.IsNullOrEmpty(successMessage))
			{
				EditorGUILayout.HelpBox(successMessage, MessageType.Info);
			}

			if (GUILayout.Button("Submit"))
			{
				SubmitReport();
			}
		}

		[MenuItem("Tools/Tezos SDK for Unity/Report an Issue")]
		public static void ShowWindow()
		{
			GetWindow<IssueReportFormWindow>("Issue Report Form");
		}

		private string GetSDKVersion()
		{
			var rootPath = Application.dataPath;
			var directories = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);

			var packageJsonPath = "";

			foreach (var directory in directories)
			{
				if (!directory.EndsWith("tezos-unity-sdk") && !directory.EndsWith("Tezos Unity SDK"))
				{
					continue;
				}

				packageJsonPath = Path.Combine(directory, "package.json");
				break;
			}

			if (!string.IsNullOrEmpty(packageJsonPath) && File.Exists(packageJsonPath))
			{
				var json = File.ReadAllText(packageJsonPath);
				var obj = JObject.Parse(json);
				return (string)obj["version"];
			}

			Debug.LogError("Issue Report Form: package.json file not found");
			return "";
		}

		private void SubmitReport()
		{
			successMessage = "";

			ValidateFields();

			if (!string.IsNullOrEmpty(errorMessage))
			{
				return;
			}

			var url =
				"https://docs.google.com/forms/d/e/1FAIpQLScUslljbVpQztjqB96D2c8dSlPpeYkM2sJdZlnOz7qyN3g4nw/formResponse?usp=pp_url";

			url += "&entry.2052348936=" + UnityWebRequest.EscapeURL(issueDefinition);
			url += "&entry.1859623235=" + UnityWebRequest.EscapeURL(stepsToReproduce);
			url += "&entry.1214054480=" + UnityWebRequest.EscapeURL(additionalContext);
			url += "&entry.1889569160=" + UnityWebRequest.EscapeURL(unityVersion);
			url += "&entry.1546304813=" + UnityWebRequest.EscapeURL(sdkVersion);
			url += "&entry.334812445=" + UnityWebRequest.EscapeURL(email);
			url += "&entry.1713563215=" + UnityWebRequest.EscapeURL(issueTypes[issueTypeIndex]);
			url += "&submit=Submit";

			Application.OpenURL(url);
			successMessage = "Report submitted. Thanks!";
		}

		private void ValidateFields()
		{
			if (string.IsNullOrEmpty(issueDefinition))
			{
				errorMessage = "Issue Definition is required.";
			}
			else if (string.IsNullOrEmpty(unityVersion))
			{
				errorMessage = "Unity Version is required.";
			}
			else if (string.IsNullOrEmpty(sdkVersion))
			{
				errorMessage = "SDK Version is required.";
			}
			else if (issueTypeIndex == 0)
			{
				errorMessage = "Issue Type is required.";
			}
			else
			{
				errorMessage = "";
			}
		}
	}

}