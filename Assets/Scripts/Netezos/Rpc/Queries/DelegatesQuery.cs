using System.Collections;
using System.Threading.Tasks;

namespace Netezos.Rpc.Queries
{
    /// <summary>
    /// Rpc query to access all registered delegates
    /// </summary>
    public class DelegatesQuery : RpcObject
    {
        /// <summary>
        /// Gets the query to the complete status of a delegate by address
        /// </summary>
        /// <param name="address">Address of the delegate</param>
        /// <returns></returns>
        public DelegateQuery this[string address] => new DelegateQuery(this, $"{address}/");

        internal DelegatesQuery(RpcQuery baseQuery, string append) : base(baseQuery, append) { }

        /// <summary>
        /// Executes the query and returns all registered delegates.
		/// Returns a dynamic json.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator GetAsync()
            => Client.GetJson($"{Query}?active=true&inactive=true");

        /// <summary>
        /// Executes the query and returns all registered delegates with the specified status.
		/// Returns a dynamic json.
        /// </summary>
        /// <param name="status">Status of the delegates to return</param>
        /// <returns></returns>
        public IEnumerator GetAsync(DelegateStatus status)
            => Client.GetJson($"{Query}?{status.ToString().ToLower()}=true");

        /// <summary>
        /// Executes the query and returns all registered delegates
        /// </summary>
        /// <returns></returns>
        public new IEnumerator GetAsync<T>()
            => Client.GetJson<T>($"{Query}?active=true&inactive=true");

        /// <summary>
        /// Executes the query and returns all registered delegates with the specified status
        /// </summary>
        /// <param name="status">Status of the delegates to return</param>
        /// <returns></returns>
        public IEnumerator GetAsync<T>(DelegateStatus status)
            => Client.GetJson<T>($"{Query}?{status.ToString().ToLower()}=true");
    }
}
