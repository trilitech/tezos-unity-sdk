using System.Collections;
using System.Threading.Tasks;

namespace Netezos.Rpc.Queries
{
    /// <summary>
    /// Retrieves the delegates allowed to endorse a block.
    /// </summary>
    public class ValidatorsQuery : RpcObject
    {
        internal ValidatorsQuery(RpcQuery baseQuery, string append) : base(baseQuery, append) { }

        /// <summary>
        /// Gives the endorsing slots for delegates that have at least one in the next block.
		/// Returns a dynamic json.
        /// </summary>
        /// <returns></returns>
        public new IEnumerator GetAsync()
            => Client.GetJson(Query);

        /// <summary>
        /// Gives the endorsing slots for the delegate
		/// Returns a dynamic json.
        /// </summary>
        /// <param name="baker">Delegate whose endorsing rights are to be returned</param>
        /// <returns></returns>
        public IEnumerator GetAsync(string baker)
            => Client.GetJson($"{Query}?delegate={baker}");

        /// <summary>
        /// Executes the query and returns the endorsing rights
		/// Returns a dynamic json.
        /// </summary>
        /// <param name="level">Level of the block at which the endorsing rights are to be returned</param>
        /// <returns></returns>
        public IEnumerator GetFromLevelAsync(int level)
            => Client.GetJson($"{Query}?level={level}");

        /// <summary>
        /// Executes the query and returns the endorsing rights
		/// Returns a dynamic json.
        /// </summary>
        /// <param name="level">Level of the block at which the endorsing rights are to be returned</param>
        /// <param name="baker">Delegate whose endorsing rights are to be returned</param>
        /// <returns></returns>
        public IEnumerator GetFromLevelAsync(int level, string baker)
            => Client.GetJson($"{Query}?level={level}&delegate={baker}");

        /// <summary>
        /// Executes the query and returns the endorsing rights
        /// </summary>
        /// <returns></returns>
        public new IEnumerator GetAsync<T>()
            => Client.GetJson<T>(Query);

        /// <summary>
        /// Executes the query and returns the endorsing rights
        /// </summary>
        /// <param name="baker">Delegate whose endorsing rights are to be returned</param>
        /// <returns></returns>
        public IEnumerator GetAsync<T>(string baker)
            => Client.GetJson<T>($"{Query}?delegate={baker}");

        /// <summary>
        /// Executes the query and returns the endorsing rights
        /// </summary>
        /// <param name="level">Level of the block at which the endorsing rights are to be returned</param>
        /// <returns></returns>
        public IEnumerator GetFromLevelAsync<T>(int level)
            => Client.GetJson<T>($"{Query}?level={level}");

        /// <summary>
        /// Executes the query and returns the endorsing rights
        /// </summary>
        /// <param name="level">Level of the block at which the endorsing rights are to be returned</param>
        /// <param name="baker">Delegate whose endorsing rights are to be returned</param>
        /// <returns></returns>
        public IEnumerator GetFromLevelAsync<T>(int level, string baker)
            => Client.GetJson<T>($"{Query}?level={level}&delegate={baker}");
    }
}
