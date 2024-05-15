using System;
using TezosSDK.Tezos.Wallet;
using UnityEngine;

namespace TezosSDK.Tezos.Models
{

	/// <summary>
	///     Represents a general structure for an event containing its type and associated data.
	///     It is used to unify event messages for processing by event handlers.
	/// </summary>
	[Serializable]
	public class UnifiedEvent
	{
		[SerializeField] private string EventType;
		[SerializeField] private string Data;

		/// <summary>
		///     Initializes a new instance of the <see cref="UnifiedEvent" /> class.
		/// </summary>
		/// <param name="eventType">
		///     Specifies the type of event.
		///     The event type is used to determine which event handler to invoke.
		///     For a list of event types, see <see cref="WalletEventManager" />.
		///     <example>
		///     </example>
		/// </param>
		/// <param name="data">
		///     Contains the data associated with the event in a JSON string format,
		///     which is further parsed in specific event handlers.
		/// </param>
		/// <example>
		///     The data for a <see cref="WalletEventManager.EventTypeWalletConnected" /> event
		///     is a <see cref="WalletInfo" /> object serialized into a JSON string.
		/// </example>
		public UnifiedEvent(string eventType, string data = null)
		{
			data ??= "{}";

			EventType = eventType;
			Data = data;
		}

		public string GetData()
		{
			return Data;
		}

		public string GetEventType()
		{
			return EventType;
		}
	}

}