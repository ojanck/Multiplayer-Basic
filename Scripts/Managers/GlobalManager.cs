using System.Collections;
using System.Collections.Generic;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Util;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer.Smartfox
{
    public class GlobalManager : MonoBehaviour
    {
        private static GlobalManager _instance;
        public static GlobalManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("Global Managers").AddComponent<GlobalManager>();
                }

                return _instance;
            }
        }

        private SmartFox sfs;
        private string connLostMsg;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Application.runInBackground = true;
            Debug.Log("Global Manager is Ready");
        }

        private void Update()
        {
            sfs?.ProcessEvents();
        }

        private void OnDestroy()
        {
            Debug.Log("Global Manager is Destroyed");
        }

        private void OnApplicationQuit()
        {
            if (sfs != null && sfs.IsConnected)
            {
                sfs.Disconnect();
            }
        }

        /**
        * <summary>
        * Return and Delete the Lost Connection Last Message
        * </summary>
        */
        public string ConnectionLostMsg
        {
            get
            {
                string m = connLostMsg;
                connLostMsg = null;
                return m;
            }
        }

        /**
        * <summary>
        * Create and Return the SmartFox instance for TCP Socket Connection
        * </summary>
        * <returns>
        * SmartFox Instance
        * </returns>
        */
        public SmartFox CreateSFSClient()
        {
            sfs = new SmartFox();
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            return sfs;
        }

        /**
        * <summary>
        * Create and Return the SmartFox instance for WebSocket Connection
        * </summary>
        * <param name="useWebSocket">
        * The Web Socket that is used
        * </param>
        * <returns>
        * SmartFox Instance
        * </returns>
        */
        public SmartFox CreateSFSClient(UseWebSocket useWebSocket)
        {
            sfs = new SmartFox(useWebSocket);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            return sfs;
        }
        
        /**
        * <summary>
        * Return the existing SmartFox class instance.
        * </summary>
        * <returns>
        * SmartFox Instance
        * </returns>
        */
        public SmartFox GetSfsClient()
        {
            return sfs;
        }

        /**
        * <summary>
        * Events that happens when Connection is Lost
        * </summary>
        */
        private void OnConnectionLost(BaseEvent evt)
        {
            // Remove CONNECTION_LOST listener
            sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            sfs = null;

            // Get disconnection reason
            string connLostReason = (string)evt.Params["reason"];

            Debug.Log("Connection to SmartFoxServer lost; reason is: " + connLostReason);

            if (SceneManager.GetActiveScene().name != "Login")
            {
                if (connLostReason != ClientDisconnectionReason.MANUAL)
                {
                    // Save disconnection message, which can be retrieved by the LOGIN scene to display an error message
                    connLostMsg = "An unexpected disconnection occurred.\n";

                    if (connLostReason == ClientDisconnectionReason.IDLE)
                        connLostMsg += "It looks like you have been idle for too much time.";
                    else if (connLostReason == ClientDisconnectionReason.KICK)
                        connLostMsg += "You have been kicked by an administrator or moderator.";
                    else if (connLostReason == ClientDisconnectionReason.BAN)
                        connLostMsg += "You have been banned by an administrator or moderator.";
                    else
                        connLostMsg += "The reason of the disconnection is unknown.";
                }

                // Switch to the LOGIN scene
                SceneManager.LoadScene("Login");
            }
        }
    }
}
