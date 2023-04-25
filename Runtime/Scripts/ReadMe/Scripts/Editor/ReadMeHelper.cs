using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace RMC.Core.ReadMe
{
	/// <summary>
	/// Helper for <see cref="ReadMe"/> 
	/// </summary>
	public static class ReadMeHelper
	{

		public static void RestartUnityEditor()
		{
			EditorApplication.OpenProject(Directory.GetCurrentDirectory());
		}
		
		
		public static List<ReadMe> SelectReadmes()
		{
			List<ReadMe> readMes = GetAllReadMes();
			
			foreach (ReadMe readMe in readMes)
			{
				SelectObject(readMe);
				PingObject(readMe);
			}

			return readMes;
		}
		
		
		
		public static void SelectObject (UnityEngine.Object obj)
		{
			EditorGUIUtility.PingObject(obj);
			Selection.objects = new[] { obj };
			Selection.activeObject = obj;
		}
		
		
		public static void PingObject (UnityEngine.Object obj)
		{
			EditorGUIUtility.PingObject(obj);
		}
		
		
		public static bool SelectReadmes_ValidationFunction()
		{
			return GetAllReadMes().Count > 0;
		}
		
		
		
		static List<ReadMe> GetAllReadMes()
		{
			AssetDatabase.Refresh();
			var ids = AssetDatabase.FindAssets("ReadMe t:ReadMe");
			List<ReadMe> results = new List<ReadMe>();

			foreach (string guid in ids)
			{
				var readmeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
				ReadMe readMe = (ReadMe)readmeObject;
				results.Add(readMe);
			}
			return results; ;
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
			string path = AssetDatabase.GetAssetPath(obj);
			GUID guid = AssetDatabase.GUIDFromAssetPath(path);
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
			ScriptableObjectUtility.CreateScriptableObject(typeof (ReadMe), newFilename);
		}


	}
}