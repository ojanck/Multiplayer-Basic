using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;

using Local.Player.Data;

namespace Multiplayer.Smartfox
{
    public class GameController : BaseSceneController
    {
        private static GameController _instance;
        public static GameController instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.Find("Controller").GetComponent<GameController>() ?? GameObject.Find("Controller").AddComponent<GameController>();
                }

                return _instance;
            }
        }

        private bool running = false;

        private SmartFox sfs;

        // private void Awake()
        // {
        //     _instance = this;
        // }

        private void Start()
        {
            // Set a reference to the SmartFox client instance
            sfs = gm.GetSfsClient();

            if (sfs == null)
            {
                SceneManager.LoadScene("Lobby");
                return;
            }

            AddSmartFoxListeners();
            SendSpawnRequest();

            running = true;
        }

        // This is needed to handle server events in queued mode
        private void FixedUpdate()
        {
            if (!running) return;
            sfs.ProcessEvents();
        }

        private void AddSmartFoxListeners()
        {
            sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
            sfs.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        }

        protected override void RemoveSmartFoxListeners()
        {
            sfs.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
            sfs.RemoveEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
            sfs.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        }

        /**
        * <summary>
        * Hide all modal panels.
        * </summary>
        */
        override protected void HideModals()
        {
            //No Modals to Hide
        }

        /**
        * <summary>
        * Send the request to server to spawn my player
        * </summary>
        */
        public void SendSpawnRequest()
        {
            Room room = sfs.LastJoinedRoom;
            ExtensionRequest request = new("spawnMe", new SFSObject(), room);
            sfs.Send(request);
        }

        /**
        * <summary>
        * Send local transform to the server
        * </summary>
        * <param name="ntransform">
        * A <see cref="TransformHandler"/>
        * </param>
        */
        public void SendTransform(Network.TransformHandler ntransform)
        {
            Room room = sfs.LastJoinedRoom;
            ISFSObject data = new SFSObject();
            ntransform.ToSFSObject(data);
            ExtensionRequest request = new("sendTransform", data, room, true); // True flag = UDP
            sfs.Send(request);
        }

        private void OnExtensionResponse(BaseEvent evt)
        {
            try
            {
                string cmd = (string)evt.Params["cmd"];
                ISFSObject dt = (SFSObject)evt.Params["params"];

                switch (cmd)
                {
                    case "spawnPlayer":
                        HandleInstantiatePlayer(dt);
                        break;
                    case "transform":
                        HandleTransform(dt);
                        break;
                    case "notransform":
                        HandleNoTransform(dt);
                        break;
                    case "time":
                        HandleServerTime(dt);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Exception handling response: " + e.Message + " >>> " + e.StackTrace);
            }
        }

        private void HandleInstantiatePlayer(ISFSObject dt)
        {
            ISFSObject playerData = dt.GetSFSObject("player");
            int userId = playerData.GetInt("id");

            User user = sfs.UserManager.GetUserById(userId);
            string name = user.Name;

            PlayerManager.instance.SpawnPlayer(userId, name, sfs.MySelf.Id == userId);
        }

        /*
        * <summary>
        * Updating transform of the remote player from server
        * </summary>
        */
        private void HandleTransform(ISFSObject dt)
        {
            int userId = dt.GetInt("id");
            Network.TransformHandler ntransform = Network.TransformHandler.FromSFSObject(dt);
            if (userId != sfs.MySelf.Id)
            {
                // Update transform of the remote user object
                PlayerData recipient = PlayerManager.instance.GetRecipient(userId);
                recipient?.ReceiveTransform(ntransform);
            }
        }

        /*
        * <summary>
        * Server rejected transform message - force the local player object to what server said
        * </summary>
        */
        private void HandleNoTransform(ISFSObject dt)
        {
            int userId = dt.GetInt("id");
            Network.TransformHandler ntransform = Network.TransformHandler.FromSFSObject(dt);

            if (userId == sfs.MySelf.Id)
            {
                // Movement restricted!
                // Update transform of the local object
                ntransform.Update(PlayerManager.instance.GetPlayerObject().transform);
            }
        }

        /*
        * <summary>
        * Synchronize the time from server
        * </summary>
        */
        private void HandleServerTime(ISFSObject dt)
        {
            long time = dt.GetLong("t");
            Network.TimeManager.Instance.Synchronize(Convert.ToDouble(time));
        }

        /*
        * <summary>
        * When a user leaves room destroy his object
        * </summary>
        */
        private void OnUserLeaveRoom(BaseEvent evt) {
            User user = (User)evt.Params["user"];
            Room room = (Room)evt.Params["room"];

            PlayerManager.instance.DestroyPlayer(user.Id);
            Debug.Log("User " + user.Name + " left");
        }

        /**
        * <summary>
        * When connection is lost we load the login scene
        * </summary>
        */
        private void OnConnectionLost(BaseEvent evt)
        {
            RemoveSmartFoxListeners();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("lobby");
        }

        /**
        * <summary>
        * Request the current server time. Used for time synchronization
        * </summary>
        */
        public void TimeSyncRequest()
        {
            Room room = sfs.LastJoinedRoom;
            ExtensionRequest request = new("getTime", new SFSObject(), room);
            sfs.Send(request);
        }
    }
}
