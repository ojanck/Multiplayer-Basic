using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Local.Player;
using Local.Player.Data;
using Meta.Data;
using Multiplayer.Smartfox.Network;

namespace Multiplayer.Smartfox
{
    public class PlayerManager : MonoBehaviour
    {
        private static PlayerManager _instance;
        public static PlayerManager instance
        {
            get
            {
                if(_instance == null)
                _instance = GameObject.Find("Controller").GetComponent<PlayerManager>() ?? GameObject.Find("Controller").AddComponent<PlayerManager>();
                return _instance;
            }
        }

        private Dictionary<int, PlayerData> recipients = new();

        [SerializeField] private GameObject playerPrefab;

        /**
        * <summary>
        * Spawn Player
        * </summary>
        * <param name="id">Player Id</param>
        * <param name="name">Player Name</param>
        */
        public void SpawnPlayer(int id, string name, bool isMyself = false)
        {
            GameObject player = Instantiate(playerPrefab);
            if (isMyself)
            {
                player.AddComponent<PlayerController>();
            }

            PlayerData playerData = player.AddComponent<PlayerData>();
            playerData.Id = id;
            playerData.Username = name;
            playerData.IsMyself = isMyself;

            if(!isMyself)
            {
                Metadata playerMeta = player.GetComponent<Metadata>();
                playerMeta.FindParam("Camera").parameter.SetActive(false);
                playerMeta.FindParamComponent<Canvas>("Canvas").worldCamera = Camera.current;

                player.AddComponent<TransformInterpolation>();
            }

            recipients[id] = playerData;
        }

        /**
        * <summary>
        * Get Player Data
        * </summary>
        * <param name="id">Player Id</param>
        */
        public PlayerData GetRecipient(int id)
        {
            return recipients.ContainsKey(id) ? recipients[id] : null;
        }

        /**
        * <summary>
        * Destroy Player
        * </summary>
        * <param name="id">Player Id</param>
        */
        public void DestroyPlayer(int id)
        {
            PlayerData rec = GetRecipient(id);
            if(rec == null) return;
            Destroy(rec.gameObject);
            recipients.Remove(id);
        }
    }
}