using System.Collections.Generic;
using System.IO;
using TezosSDK.ReadMe.Scripts.Runtime;
using UnityEditor;
using UnityEngine;

namespace TezosSDK.ReadMe.Scripts.Editor
{

	/// <summary>
	///     Helper for <see cref="ReadMe" />
	/// </summary>
	public static class ReadMeHelper
	{
		public static void RestartUnityEditor()
		{
			EditorApplication.OpenProject(Directory.GetCurrentDirectory());
		}

		public static List<Runtime.ReadMe> SelectReadmes()
		{
			var readMes = GetAllReadMes();

			foreach (var readMe in readMes)
			{
				SelectObject(readMe);
				PingObject(readMe);
			}

			return readMes;
		}

		public static void SelectObject(Object obj)
		{
			EditorGUIUtility.PingObject(obj);

			Selection.objects = new[]
			{
				obj
			};

			Selection.activeObject = obj;
		}

		public static void PingObject(Object obj)
		{
			EditorGUIUtility.PingObject(obj);
		}

		public static bool SelectReadmes_ValidationFunction()
		{
			return GetAllReadMes().Count > 0;
		}

		private static List<Runtime.ReadMe> GetAllReadMes()
		{
			AssetDatabase.Refresh();
			var ids = AssetDatabase.FindAssets("ReadMe t:ReadMe");
			var results = new List<Runtime.ReadMe>();

			foreach (var guid in ids)
			{
				var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
				var readMe = (Runtime.ReadMe)readmeObject;
				results.Add(readMe);
			}

			return results;
			;
		}

		public static void CopyGuidToClipboard()
		{
			// Support only if exactly 1 object is selected in project window
			var objs = Selection.objects;

			if (objs.Length != 1)
			{
				return;
			}

			var obj = objs[0];
			var path = AssetDatabase.GetAssetPath(obj);
			var guid = AssetDatabase.GUIDFromAssetPath(path);
			GUIUtility.systemCopyBuffer = guid.ToString();
			Debug.Log($"CopyGuidToClipboard() success! Value '{GUIUtility.systemCopyBuffer}' copied to clipboard.");
		}

		public static bool CopyGuidToClipboard_ValidationFunction()
		{
			// Support only if exactly 1 object is selected in project window
			var objs = Selection.objects;
			return objs.Length == 1;
		}

		public static void CreateNewReadMe(string newFilename = "")
		{
			ScriptableObjectUtility.CreateScriptableObject(typeof(Runtime.ReadMe), newFilename);
		}
	}

}