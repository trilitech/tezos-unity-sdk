using TezosSDK.ReadMe.Scripts.Runtime;
using UnityEditor;
using UnityEngine;

namespace TezosSDK.ReadMe.Scripts.Editor
{

	/// <summary>
	///     Editor to render <see cref="ReadMe" /> in the Unity Inspector
	/// </summary>
	[CustomEditor(typeof(Runtime.ReadMe))]
	[InitializeOnLoad]
	public class ReadMeEditor : UnityEditor.Editor
	{
		[SerializeField] private GUIStyle TitleStyle;
		[SerializeField] private GUIStyle IconStyle;
		[SerializeField] private GUIStyle TextHeadingStyle;
		[SerializeField] private GUIStyle TextSubheadingStyle;
		[SerializeField] private GUIStyle TextBodyStyle;
		[SerializeField] private GUIStyle LinkTextStyle;

		private static readonly bool _isInActiveDevelopment = false; //set false for production
		private static readonly float hSpace1 = 8;
		private static readonly float hSpace2 = 16;
		private static readonly float hSpace3 = 25;
		private static readonly float LayoutMaxWidth = 400; //Most, not all items
		private static readonly float LayoutMinWidht = 250; //Most, not all items
		private static readonly float vSpaceAfterEachSection = 10f;
		private static readonly float VSpaceBeforeAllSections = 5;
		private static bool _isInitialized;

		private ReadMeEditor()
		{
			_isInitialized = false;
		}

		public override void OnInspectorGUI()
		{
			var readMe = (Runtime.ReadMe)target;

			Initialize();

			var MinWidth = Mathf.Clamp(EditorGUIUtility.currentViewWidth, LayoutMinWidht, LayoutMaxWidth);

			if (readMe != null && readMe.Sections != null)
			{
				GUILayout.Space(VSpaceBeforeAllSections);

				foreach (var section in readMe.Sections)
				{
					if (section == null)
					{
						continue;
					}

					if (!string.IsNullOrEmpty(section.TextHeading))
					{
						GUILayout.Space(5);
						GUILayout.BeginHorizontal(GUILayout.Width(MinWidth));
						GUILayout.Space(hSpace1);
						GUILayout.Label(section.TextHeading, TextHeadingStyle);
						GUILayout.EndHorizontal();
						GUILayout.Space(3);
					}

					if (!string.IsNullOrEmpty(section.TextSubheading))
					{
						GUILayout.Space(5);
						GUILayout.BeginHorizontal(GUILayout.Width(MinWidth));
						GUILayout.Space(hSpace2);
						GUILayout.Label(section.TextSubheading, TextSubheadingStyle);
						GUILayout.EndHorizontal();
						GUILayout.Space(3);
					}

					if (!string.IsNullOrEmpty(section.TextBody))
					{
						GUILayout.BeginHorizontal(GUILayout.Width(MinWidth));
						GUILayout.Space(hSpace3);
						GUILayout.TextField(ProcessText(section.TextBody), TextBodyStyle);
						GUILayout.EndHorizontal();
						GUILayout.Space(3);
					}

					if (!string.IsNullOrEmpty(section.LinkName))
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(hSpace3);
						GUILayout.Label("▶");

						if (ProcessLink(new GUIContent(section.LinkName)))
						{
							Application.OpenURL(section.LinkUrl);
						}

						GUILayout.Space(1000);
						GUILayout.EndHorizontal();
					}

					if (!string.IsNullOrEmpty(section.PingObjectName))
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(hSpace3);
						GUILayout.Label("▶");

						if (ProcessLink(new GUIContent(section.PingObjectName)))
						{
							var path = AssetDatabase.GUIDToAssetPath(section.PingObjectGuid);
							var objectToSelect = AssetDatabase.LoadAssetAtPath<Object>(path);
							EditorGUIUtility.PingObject(objectToSelect);

							// Do not select it.
							// Since For most users that would un-select the ReadMe.asset and disorient user
							// Selection.activeObject = objectToSelect;
						}

						GUILayout.Space(1000);
						GUILayout.EndHorizontal();
					}

					if (!string.IsNullOrEmpty(section.MenuItemName))
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(hSpace3);
						GUILayout.Label("▶");

						if (ProcessLink(new GUIContent(section.MenuItemName)))
						{
							EditorApplication.ExecuteMenuItem(section.MenuItemPath);
						}

						GUILayout.Space(1000);
						GUILayout.EndHorizontal();
					}

					GUILayout.Space(vSpaceAfterEachSection);
				}
			}
		}

		protected override void OnHeaderGUI()
		{
			var readMe = (Runtime.ReadMe)target;

			Initialize();

			var iconWidth = Mathf.Min(EditorGUIUtility.currentViewWidth / 3f - 20f, 100);
			var labelWidth = EditorGUIUtility.currentViewWidth - iconWidth;
			var labelMinWidth = 200;
			var headerHeight = 85;
			iconWidth = 80;

			GUILayout.BeginHorizontal(GUILayout.MaxHeight(headerHeight));

			{
				IconStyle.fixedWidth = iconWidth;
				IconStyle.fixedHeight = iconWidth;
				GUILayout.Box(readMe.Icon, IconStyle);

				GUILayout.Label(ProcessText(readMe.Title), TitleStyle, GUILayout.MaxWidth(labelWidth),
					GUILayout.MinWidth(labelMinWidth));
			}

			GUILayout.EndHorizontal();
			GUIDividerLine();
		}

		private void GUIDividerLine(int height = 1)
		{
			var rect = EditorGUILayout.GetControlRect(false, height);
			rect.height = height;
			//Line
			EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, .8f));

			//Dropshadow
			rect.y += 2;
			EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, .4f));
		}

		private void Initialize()
		{
			if (_isInitialized)
			{
				return;
			}

			//In active development, comment this out to constantly
			//refresh the style data
			if (_isInActiveDevelopment)
			{
				Debug.Log("Initialize. TODO: Set IsInActiveDevelopment = false");
			}
			else
			{
				_isInitialized = true;
			}

			//Declare Styles
			TitleStyle = new GUIStyle(EditorStyles.label)
			{
				stretchHeight = true,
				wordWrap = true,
				fontSize = 20,
				margin =
				{
					left = 10
				},
				alignment = TextAnchor.MiddleLeft
			};

			//Icon
			IconStyle = new GUIStyle(EditorStyles.iconButton)
			{
				normal =
				{
					background = null
				},
				hover =
				{
					background = null
				},
				active =
				{
					background = null
				},
				margin = new RectOffset(5, 5, 5, 5),
				alignment = TextAnchor.MiddleCenter
			};

			//TextHeading
			TextHeadingStyle = new GUIStyle(TitleStyle)
			{
				wordWrap = true,
				fontSize = 20
			};

			//TextSubheadingStyle
			TextSubheadingStyle = new GUIStyle(TextHeadingStyle)
			{
				wordWrap = true,
				fontStyle = FontStyle.Bold,
				fontSize = 18
			};

			//TextBodyStyle - Supports richText (https://docs.unity3d.com/2021.3/Documentation/Manual/StyledText.html)
			TextBodyStyle = new GUIStyle(TextHeadingStyle)
			{
				wordWrap = true,
				richText = true,
				fontSize = 12,
				border = new RectOffset(0, 0, 0, 0)
			};

			//LinkTextStyle
			LinkTextStyle = new GUIStyle(EditorStyles.linkLabel)
			{
				wordWrap = false,
				stretchWidth = false
			};
		}

		/// <summary>
		///     Format the links pretty
		/// </summary>
		private bool ProcessLink(GUIContent label, params GUILayoutOption[] options)
		{
			var position = GUILayoutUtility.GetRect(label, LinkTextStyle, options);

			Handles.BeginGUI();
			Handles.color = LinkTextStyle.normal.textColor;
			Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
			Handles.color = Color.white;
			Handles.EndGUI();

			EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

			return GUI.Button(position, label, LinkTextStyle);
		}

		/// <summary>
		///     All for "\n" in the source to be a line break in the result
		/// </summary>
		private string ProcessText(string s)
		{
			return s.Replace("\\n", "\n");
		}
	}

}