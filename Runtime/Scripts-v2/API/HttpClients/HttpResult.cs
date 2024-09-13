namespace TezosSDK.API
{

	public class HttpResult<T>
	{
		public HttpResult(T data)
		{
			Data = data;
			Success = true;
			ErrorMessage = string.Empty;
		}

		public HttpResult(string errorMessage)
		{
			Data = default;
			Success = false;
			ErrorMessage = errorMessage;
		}

		public T Data { get; private set; }
		public bool Success { get; private set; }
		public string ErrorMessage { get; private set; }
	}

}