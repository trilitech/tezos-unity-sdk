namespace TezosSDK.ReadMe.Scripts.Runtime
{

	/// <summary>
	///     Common values for <see cref="ReadMe" />
	/// </summary>
	public static class ReadMeConstants
	{
		//  Properties ------------------------------------

		//  Fields ----------------------------------------
		public const string MenuItemPathCreate = CompanyName + "/" + ProjectName;
		public const string MenuItemPathWindow = "Window/" + CompanyName + "/" + ProjectName;

		//
		public const string CompanyName = "RMC";
		public const string ProjectName = "ReadMe";
		public const string Open = "Open";

		//
		public const int MenuPriority_Primary = 0; //put above divider line
		public const int MenuPriority_Secondary = 100; //put below divider line
	}

}