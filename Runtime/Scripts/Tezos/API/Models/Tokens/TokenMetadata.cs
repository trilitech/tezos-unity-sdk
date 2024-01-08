using System.Collections.Generic;
using TezosSDK.Helpers.Extensions;

namespace TezosSDK.Tezos.API.Models.Tokens
{

	public class TokenMetadata
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Symbol { get; set; }
		public string Decimals { get; set; }
		public string DisplayUri { get; set; }
		public string ArtifactUri { get; set; }
		public string ThumbnailUri { get; set; }

		// todo: add creators.
		// public IEnumerable<string> Creators { get; set; }

		public IDictionary<string, byte[]> GetMetadataDict()
		{
			var dict = new Dictionary<string, byte[]>
			{
				{
					nameof(Name).FirstCharToLowerCase(), Name.ToByteArray()
				},
				{
					nameof(Description).FirstCharToLowerCase(), Description.ToByteArray()
				},
				{
					nameof(Symbol).FirstCharToLowerCase(), Symbol.ToByteArray()
				},
				{
					nameof(Decimals).FirstCharToLowerCase(), Decimals.ToByteArray()
				},
				{
					nameof(DisplayUri).FirstCharToLowerCase(), DisplayUri.ToByteArray()
				},
				{
					nameof(ArtifactUri).FirstCharToLowerCase(), ArtifactUri.ToByteArray()
				},
				{
					nameof(ThumbnailUri).FirstCharToLowerCase(), ThumbnailUri.ToByteArray()
				}
			};

			return new SortedDictionary<string, byte[]>(dict);
		}

		public override string ToString()
		{
			return $"Name: {Name}, " + $"Description: {Description}, " + $"Symbol: {Symbol}, " +
			       $"Decimals: {Decimals}, " + $"DisplayUri: {DisplayUri}, " + $"ArtifactUri: {ArtifactUri}, " +
			       $"ThumbnailUri: {ThumbnailUri}";
		}
	}

}