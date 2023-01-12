using System;
using System.Collections.Generic;
using UnityEngine;
using Meta.Data.Params;

namespace Meta.Data
{
    /**
    * <summary>
    * Sebuah Script Berisikan Fungsi Untuk Memanggil Data Dari Sebuah Objek
    * Editor By : Izzan A.F.
    * </summary>
    */
    public class Metadata : MonoBehaviour
    {
        [Tooltip("List of Parameters")]
        [SerializeField] List<MetaParam> _params;
        
        /**
        * <summary>
        * Mendapatkan Parameter Dari Data Yang Di-inginkan
        * </summary>
        * <returns>MetaParam</returns>
        * <param name="parameter">Paramater Yang Telah Dipilih</param>
        */
        public MetaParam FindParam(string parameter)
        {
            var param = _params.Find(x => x.id == parameter);
            if(param == null) return null;

            return param;
        }

        /**
        * <summary>
        * Mendapatkan Component Dari Parameter Yang Di-inginkan
        * </summary>
        * <returns>Component</returns>
        * <param name="parameter">Paramater Yang Telah Dipilih</param>
        */
        public Component FindParamComponent<Component>(string parameter) => FindParam(parameter).parameter.GetComponent<Component>();
    }
}