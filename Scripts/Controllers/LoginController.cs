using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using Sfs2X;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Requests;

namespace Multiplayer.Smartfox
{
    public class LoginController : BaseSceneController
    {
        [Header("Json")]
        [Tooltip("Room Setting File (.json / .txt) Location")]
        [TextArea(1, 10)] [SerializeField] private string roomSettingLocation;
        private string roomSettingJson;

        [Header("Canvas")]
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private Button loginButton;

        [Header("Server")]
        [SerializeField] private int TCPPort = 9933;
        [SerializeField] private int HTTPPort = 8080;
        [SerializeField] private bool debug = false;
        private const string ROOM_NAME = "MMOBasics";

        private SmartFox sfs;

        //----------------------------------------------------------
        // Unity calback methods
        //----------------------------------------------------------

        private void Start()
        {
            // Focus on username input
            nameInput.Select();
            nameInput.ActivateInputField();

            // Show connection lost message, in case the disconnection occurred in another scene
            string connLostMsg = gm.ConnectionLostMsg;
            if (connLostMsg != null)
                errorText.text = connLostMsg;
        }

        //----------------------------------------------------------
        // UI event listeners
        //----------------------------------------------------------
        #region
        /**
        * On username input edit end, if the Enter key was pressed, connect to SmartFoxServer.
        */
        public void OnNameInputEndEdit()
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                Connect();
        }

        /**
        * On Login button click, connect to SmartFoxServer.
        */
        public void OnLoginButtonClick()
        {
            Connect();
        }
        #endregion

        //----------------------------------------------------------
        // Helper methods
        //----------------------------------------------------------
        #region
        /**
        * Enable/disable username input interaction.
        */
        private void EnableUI(bool enable)
        {
            nameInput.interactable = enable;
            loginButton.interactable = enable;
        }

        /**
        * Connect to SmartFoxServer.
        */
        private void Connect()
        {
            // Disable user interface
            EnableUI(false);

            // Clear any previour error message
            errorText.text = "";

            // Get Config Json
            roomSettingJson = ReadTextFile(roomSettingLocation);
            ConfigData configJson = JsonConvert.DeserializeObject<ConfigData>(roomSettingJson);

            // Set connection parameters
            ConfigData cfg = new()
            {
                Host = configJson.Host,
                Port = configJson.Port,
                UdpHost = configJson.Host,
                UdpPort = configJson.UdpPort,
                Zone = configJson.Zone,
                Debug = configJson.Debug
            };

            // Initialize SmartFox client
            // The singleton class GlobalManager holds a reference to the SmartFox class instance,
            // so that it can be shared among all the scenes
            sfs = gm.CreateSFSClient();

            // Configure SmartFox internal logger
            sfs.Logger.EnableConsoleTrace = debug;

            // Add event listeners
            AddSmartFoxListeners();

            // Connect to SmartFoxServer
            sfs.Connect(cfg);
        }

        /**
        * <summary>
        * Read a Text (.txt / .json) File
        * </summary>
        * <param name="filePath">
        * The Path of the .txt File
        * </param>
        * <returns>
        * string
        * </returns>
        */
        private string ReadTextFile(string filePath)
        {
            string line = string.Empty;
            try
            {
                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                using StreamReader sr = new(filePath);
                // Read and display lines from the file until the end of
                // the file is reached.
                line = sr.ReadToEnd();

                sr.Close();
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Debug.Log("The file could not be read:");
                Debug.Log(e.Message);
                line = string.Empty;
            }

            return line;
        }

        /**
        * Add all SmartFoxServer-related event listeners required by the scene.
        */
        private void AddSmartFoxListeners()
        {
            sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
            sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
            sfs.AddEventListener(SFSEvent.UDP_INIT, OnUdpInit);
        }

        /**
        * Remove all SmartFoxServer-related event listeners added by the scene.
        * This method is called by the parent BaseSceneController.OnDestroy method when the scene is destroyed.
        */
        override protected void RemoveSmartFoxListeners()
        {
            // NOTE
            // If this scene is stopped before a connection is established, the SmartFox client instance
            // could still be null, causing an error when trying to remove its listeners

            if (sfs != null)
            {
                sfs.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
                sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
                sfs.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
                sfs.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
                sfs.RemoveEventListener(SFSEvent.UDP_INIT, OnUdpInit);
            }
        }

        /**
        * Hide all modal panels.
        */
        override protected void HideModals()
        {
            // No modals used by this scene
        }
        #endregion

        //----------------------------------------------------------
        // SmartFoxServer event listeners
        //----------------------------------------------------------
        #region
        private void OnConnection(BaseEvent evt)
        {
            // Check if the conenction was established or not
            if ((bool)evt.Params["success"])
            {
                Debug.Log("SFS2X API version: " + sfs.Version);
                Debug.Log("Connection mode is: " + sfs.ConnectionMode);

                // Login
                sfs.Send(new LoginRequest(nameInput.text));
            }
            else
            {
                // Show error message
                errorText.text = "Connection failed; is the server running at all?";

                // Enable user interface
                EnableUI(true);
            }
        }

        private void OnConnectionLost(BaseEvent evt)
        {
            // Remove SFS listeners
            RemoveSmartFoxListeners();

            // Show error message
            string reason = (string)evt.Params["reason"];

            if (reason != ClientDisconnectionReason.MANUAL)
                errorText.text = "Connection lost; reason is: " + reason;

            // Enable user interface
            EnableUI(true);
        }

        private void OnLogin(BaseEvent evt)
        {
            // Initialize UDP communication
            sfs.InitUDP();
        }

        private void OnLoginError(BaseEvent evt)
        {
            // Disconnect
            // NOTE: this causes a CONNECTION_LOST event with reason "manual", which in turn removes all SFS listeners
            sfs.Disconnect();

            // Show error message
            errorText.text = "Login failed due to the following error:\n" + (string)evt.Params["errorMessage"];

            // Enable user interface
            EnableUI(true);
        }

        private void OnUdpInit(BaseEvent evt)
        {
            if ((bool)evt.Params["success"])
            {
                // Load lobby scene
                SceneManager.LoadScene("Lobby");
            }
            else
            {
                // Disconnect
                // NOTE: this causes a CONNECTION_LOST event with reason "manual", which in turn removes all SFS listeners
                sfs.Disconnect();

                // Show error message
                errorText.text = "UDP initialization failed due to the following error:\n" + (string)evt.Params["errorMessage"];

                // Enable user interface
                EnableUI(true);
            }
        }
        #endregion
    }
}