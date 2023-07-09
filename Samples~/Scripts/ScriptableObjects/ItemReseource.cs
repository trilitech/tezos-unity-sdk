using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TezosSDK.Samples
{
    [CreateAssetMenu(fileName = "ItemResources", menuName = "ScriptableObjects/ItemResources", order = 1)]
    public class ItemReseource : ScriptableObject
    {
        public string Name;
        public Sprite ItemSprite;
    }
}