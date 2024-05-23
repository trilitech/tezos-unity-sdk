using System;
using TezosSDK.ReadMe.Scripts.Runtime;
using UnityEditor;
using UnityEngine;

namespace TezosSDK.ReadMe.Scripts.Editor
{

	/// <summary>
	///     Reflection is string-based and Unity-version-based and thus, notoriously fragile.
	///     This wraps uses for <see cref="ReadMe" /> to limit risk and help maintainability
	/// </summary>
	public static class ReadMeReflectionUtility
	{
		/// <summary>
		///     Load a unity editor layout by path
		/// </summary>
		/// <param name="path"></param>
		public static void UnityEditor_WindowLayout_LoadWindowLayout(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogWarning($"UnityEditor_WindowLayout_LoadWindowLayout() failed for path = {path}.");
				return;
			}

			var assembly = typeof(EditorApplication).Assembly;
			var windowLayoutType = assembly.GetType("UnityEditor.WindowLayout", true);
			var methods = windowLayoutType.GetMethods();

			for (var i = 0; i < methods.Length; i++)
			{
				if (methods[i].Name == "LoadWindowLayout")
				{
					// As with all Unity reflection, this is relatively fragile.
					// Use test code here if/when needed to debug.
					//
					// Debug.Log("methods: " + methods[i].Name  + " and " + methods[i].GetParameters().Length + " \n\n");
					// for (int j = 0; j < methods[i].GetParameters().Length; j++)
					// {
					// 	Debug.Log("\tparams: " + methods[i].GetParameters()[j].Name  + " and " + methods[i].GetParameters()[j].ParameterType + " \n\n");
					// }

					// Tested with success in Unity 2020.3.34f1
					if (methods[i].GetParameters().Length == 2)
					{
						try
						{
							methods[i].Invoke(null, new object[]
							{
								path, false
							});
						}
						catch (Exception e)
						{
							Debug.LogError(e.Message);
						}
					}
				}
			}
		}
	}

}