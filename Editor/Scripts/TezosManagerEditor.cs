using TezosSDK.Tezos;
using UnityEditor;
using UnityEngine;

namespace TezosSDK.Editor.Scripts
{

	[CustomEditor(typeof(TezosManager))]
	public class TezosManagerEditor : UnityEditor.Editor
	{
		private Texture2D logo;

		private void OnEnable()
		{
			logo = (Texture2D)Resources.Load("tezos-logo");
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Tezos SDK Manager", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label(logo, GUILayout.Width(90), GUILayout.Height(90));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			// Draw fields excluding the script reference
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
		}
	}

}