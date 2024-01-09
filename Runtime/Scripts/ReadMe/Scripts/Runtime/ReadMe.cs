using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TezosSDK.ReadMe.Scripts.Runtime
{

	/// <summary>
	///     Custom-formatted ReadMe file with markdown-like display.
	///     Inspired by Unity's "Learn" Sample Projects
	/// </summary>
	public class ReadMe : ScriptableObject
	{
		//Here are TEMPORARY defaults. Set to any value (or "") in the INSPECTOR as desired.

		//Header
		[FormerlySerializedAs("title")] public string Title = "ReadMe";
		public Texture2D Icon;

		//Body
		public Section[] Sections =
		{
			new(), new()
		};

		//History
		public bool HasLoadedLayout;

		#region Nested Types

		[Serializable]
		public class Section
		{
			//Here are TEMPORARY defaults. Set to any value (or "") in the INSPECTOR as desired.
			public string TextHeading = "Text Heading";
			public string TextSubheading = "Text Subheading";
			public string TextBody =
				"Text Body which supports rich text including <b>bold</b>, line\nbreaks, <i>italics</i>, & <color='black'>colors</color>.";

			//Set LinkName to "" to disable
			public string LinkName = "Link Text";
			public string LinkUrl = "http://www.Google.com";

			//Set PingObjectName to "" to disable
			public string PingObjectName = "Ping Object Name";
			public string PingObjectGuid = "5089deeea4e23e14681cf62c947ca464";

			//Set MenuItemName to "" to disable
			public string MenuItemName = "Menu Item Name";
			public string MenuItemPath = "Window/Package Manager";
		}

		#endregion
	}

}