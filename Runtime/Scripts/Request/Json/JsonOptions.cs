using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tezos.Request
{

	public static class JsonOptions
	{
		public static readonly JsonSerializerOptions DefaultOptions = new()
		{
			AllowTrailingCommas = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			MaxDepth = 100_000,
			NumberHandling = JsonNumberHandling.AllowReadingFromString,
			PropertyNamingPolicy = new SnakeCaseNamingPolicy()
		};
	}

}