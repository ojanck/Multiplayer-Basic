using System;
using UnityEngine;

namespace Meta.Data.Params
{
    /// <summary>
    /// Sebuah Script Berisikan Parameter Dari Sebuah Objek
    /// Editor By : Izzan A.F.
    /// </summary>
    [Serializable]
    public class MetaParam
    {
        [Tooltip("Id dari Object")]
        public string id;

        [Tooltip("Parameter dari Object")]
        public GameObject parameter;
    }
}