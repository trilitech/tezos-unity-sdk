using System;
using System.Collections.Generic;
using System.Linq;

namespace TezosSDK.Reflection
{
	public static class ReflectionHelper
	{
		/// <summary>
		/// Creates Instances Of Every Type Which Inherits From T Interface
		/// </summary>
		/// <param name="exceptTypes">Types To Not Crease Instances Of</param>
		/// <typeparam name="T">Interface Type To Create</typeparam>
		/// <returns></returns>
		public static IEnumerable<T> CreateInstancesOfType<T>(params Type[] exceptTypes)
		{
			Type interfaceType = typeof(T);
			IEnumerable<object> result = AppDomain.CurrentDomain.GetAssemblies()
												  .SelectMany(x => x.GetTypes())
												  .Where(x => interfaceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && exceptTypes.All(type => x != type))
												  .Select(Activator.CreateInstance);

			return result.Cast<T>();
		}
	}
}
