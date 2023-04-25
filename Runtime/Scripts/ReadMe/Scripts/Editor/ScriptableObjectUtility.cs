﻿using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace RMC.Core.ReadMe
{
	/// <summary>
	/// Helper for <see cref="ReadMe"/> 
	/// </summary>

	public class ScriptableObjectUtility : ScriptableObject
	{
		public static void CreateScriptableObject(Type type, string newFilename = "")
		{
			UnityEngine.Object asset = (UnityEngine.Object)ScriptableObject.CreateInstance(type);

			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}
			
			// Use the name passed in, or a default
			if (string.IsNullOrEmpty(newFilename))
			{
				newFilename = $"New {type.Name}";
			}
			
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + newFilename + ".asset");

			AssetDatabase.CreateAsset(asset, assetPathAndName);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}
	}
}