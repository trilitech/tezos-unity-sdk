using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TezosSDK.Samples.DemoExample
{
    public class StatsView : MonoBehaviour
    {
        [SerializeField] private StatTextView statTextPrefab;

        public void DisplayStats(StatParams stats)
        {
            Clear();

            PropertyInfo[] pis = stats.GetType().GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                StatTextView newStat = Instantiate(statTextPrefab, transform);
                // convert name from camel case to sentence case
                string sentName = Regex.Replace(pi.Name, @"\p{Lu}", m => " " + m.Value);
                newStat.SetStatName(sentName);
                newStat.SetStatValue(pi.GetValue(stats).ToString());
            }
        }

        public void Clear()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
