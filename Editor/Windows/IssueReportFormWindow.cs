using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace TezosSDK.Editor.Windows
{

	public class IssueReportFormWindow : EditorWindow
	{
		private readonly string[] _issueTypes =
		{
			"Select a issue type...", "Fix", "Critical Bug", "Bug", "Feature", "Other"
		};

		private string _additionalContext = "";
		private string _email = "";
		private string _errorMessage = "";
		private string _issueDefinition = "";
		private int _issueTypeIndex;
		private string _sdkVersion = "";
		private string _stepsToReproduce = "";
		private string _successMessage = "";
		private string _unityVersion = "";

		private void OnGUI()
		{
			GUILayout.Label("Issue Report Form", EditorStyles.boldLabel);
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Issue Definition*", GUILayout.Width(100));
			_issueDefinition = EditorGUILayout.TextArea(_issueDefinition, GUILayout.Height(60));
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Steps To Reproduce (Optional)", GUILayout.Width(200));
			_stepsToReproduce = EditorGUILayout.TextArea(_stepsToReproduce, GUILayout.Height(60));
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Additional Context (Optional)", GUILayout.Width(200));
			_additionalContext = EditorGUILayout.TextArea(_additionalContext, GUILayout.Height(60));
			EditorGUILayout.Space();

			if (string.IsNullOrEmpty(_unityVersion))
			{
				_unityVersion = Application.unityVersion;
			}

			_unityVersion = EditorGUILayout.TextField("Unity Version*", _unityVersion);
			EditorGUILayout.Space();

			if (string.IsNullOrEmpty(_sdkVersion))
			{
				_sdkVersion = GetSDKVersion();
			}

			_sdkVersion = EditorGUILayout.TextField("SDK Version*", _sdkVersion);
			EditorGUILayout.Space();

			_email = EditorGUILayout.TextField("Email (Optional)", _email);
			EditorGUILayout.Space();

			_issueTypeIndex = EditorGUILayout.Popup("Issue Type*", _issueTypeIndex, _issueTypes);
			EditorGUILayout.Space();

			ValidateFields();

			if (!string.IsNullOrEmpty(_errorMessage))
			{
				EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);
			}

			if (!string.IsNullOrEmpty(_successMessage))
			{
				EditorGUILayout.HelpBox(_successMessage, MessageType.Info);
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
			_successMessage = "";

			ValidateFields();

			if (!string.IsNullOrEmpty(_errorMessage))
			{
				return;
			}

			var url = "https://docs.google.com/forms/d/e/1FAIpQLScUslljbVpQztjqB96D2c8dSlPpeYkM2sJdZlnOz7qyN3g4nw/formResponse?usp=pp_url";

			url += "&entry.2052348936=" + UnityWebRequest.EscapeURL(_issueDefinition);
			url += "&entry.1859623235=" + UnityWebRequest.EscapeURL(_stepsToReproduce);
			url += "&entry.1214054480=" + UnityWebRequest.EscapeURL(_additionalContext);
			url += "&entry.1889569160=" + UnityWebRequest.EscapeURL(_unityVersion);
			url += "&entry.1546304813=" + UnityWebRequest.EscapeURL(_sdkVersion);
			url += "&entry.334812445=" + UnityWebRequest.EscapeURL(_email);
			url += "&entry.1713563215=" + UnityWebRequest.EscapeURL(_issueTypes[_issueTypeIndex]);
			url += "&submit=Submit";

			Application.OpenURL(url);
			_successMessage = "Report submitted. Thanks!";
		}

		private void ValidateFields()
		{
			if (string.IsNullOrEmpty(_issueDefinition))
			{
				_errorMessage = "Issue Definition is required.";
			}
			else if (string.IsNullOrEmpty(_unityVersion))
			{
				_errorMessage = "Unity Version is required.";
			}
			else if (string.IsNullOrEmpty(_sdkVersion))
			{
				_errorMessage = "SDK Version is required.";
			}
			else if (_issueTypeIndex == 0)
			{
				_errorMessage = "Issue Type is required.";
			}
			else
			{
				_errorMessage = "";
			}
		}
	}

}