using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Requests;
using TMPro;
using Meta.Data;

namespace Multiplayer.Smartfox
{
    /**
    * <summary>
    * Script attached to the Controller object in the Lobby scene.
    * </summary>
    */
    public class LobbyController : BaseSceneController
    {
        private static string GAME_ROOMS_GROUP_NAME = "games";
        private const string EXTENSION_ID = "FirstPersonShooter";
        private const string EXTENSION_CLASS = "dk.fullcontrol.fps.FpsExtension";

        //----------------------------------------------------------
        // UI elements
        //----------------------------------------------------------

        public TextMeshProUGUI loggedInAsLabel;

        public GameObject warningPanel;

        public Transform gameListContent;
        public GameListItem gameListItemPrefab;

        //----------------------------------------------------------
        // Private properties
        //----------------------------------------------------------

        private SmartFox sfs;
        private Dictionary<int, GameListItem> gameListItems;

        //----------------------------------------------------------
        // Unity calback methods
        //----------------------------------------------------------

        private void Start()
        {
            // Set a reference to the SmartFox client instance
            sfs = gm.GetSfsClient();

            if (sfs == null)
            {
                SceneManager.LoadScene("Login");
                return;
            }

            // Hide modal panels
            HideModals();

            // Display username in footer
            loggedInAsLabel.text = "Logged in as <b>" + sfs.MySelf.Name + "</b>";

            // Add event listeners
            AddSmartFoxListeners();

            // Populate list of available games
            PopulateGamesList();
        }

        //----------------------------------------------------------
        // UI event listeners
        //----------------------------------------------------------
        #region
        /**
        * <summary>
        * On Logout button click, disconnect from SmartFoxServer.
        * This causes the SmartFox listeners added by this scene to be removed (see BaseSceneController.OnDestroy method)
        * and the Login scene to be loaded (see GlobalManager.OnConnectionLost method).
        * </summary>
        */
        public void OnLogoutButtonClick()
        {
            // Disconnect from SmartFoxServer
            sfs.Disconnect();
        }

        /**
        * <summary>
        * On Start game button click, create and join a new game Room.
        * </summary>
        */
        public void OnStartGameButtonClick()
        {
            // Configure Room
            RoomSettings settings = new RoomSettings(sfs.MySelf.Name + "'s game");
            settings.GroupId = GAME_ROOMS_GROUP_NAME;
            settings.IsGame = true;
            settings.MaxUsers = 4;
            settings.MaxSpectators = 0;
            settings.Extension = new RoomExtension(EXTENSION_ID, EXTENSION_CLASS);

            // Request Room creation to server
            sfs.Send(new CreateRoomRequest(settings, true, sfs.LastJoinedRoom));
        }

        /**
        * <summary>
        * On Play game button click in Game List Item prefab instance, join an existing game Room as a player.
        * </summary>
        */
        public void OnGameItemPlayClick(int roomId)
        {
            // Join game Room as player
            sfs.Send(new Sfs2X.Requests.JoinRoomRequest(roomId));
        }
        #endregion

        //----------------------------------------------------------
        // Helper methods
        //----------------------------------------------------------
        #region
        /**
        * <summary>
        * Add all SmartFoxServer-related event listeners required by the scene.
        * </summary>
        */
        private void AddSmartFoxListeners()
        {
            sfs.AddEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
            sfs.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
            sfs.AddEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
            sfs.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChanged);
            sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
        }

        /**
        * <summary>
        * Remove all SmartFoxServer-related event listeners added by the scene.
        * This method is called by the parent BaseSceneController.OnDestroy method when the scene is destroyed.
        * </summary>
        */
        override protected void RemoveSmartFoxListeners()
        {
            sfs.RemoveEventListener(SFSEvent.ROOM_CREATION_ERROR, OnRoomCreationError);
            sfs.RemoveEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);
            sfs.RemoveEventListener(SFSEvent.ROOM_REMOVE, OnRoomRemoved);
            sfs.RemoveEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChanged);
            sfs.RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.RemoveEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
        }

        /**
        * <summary>
        * Hide all modal panels.
        * </summary>
        */
        override protected void HideModals()
        {
            warningPanel.SetActive(false);
        }

        /**
        * <summary>
        * Display list of existing games.
        * </summary>
        */
        private void PopulateGamesList()
        {
            // Initialize list
            gameListItems ??= new Dictionary<int, GameListItem>();

            // For the game list we use a scrollable area containing a separate prefab for each Game Room
            // The prefab contains clickable buttons to join the game
            List<Room> rooms = sfs.RoomManager.GetRoomList();

            // Display game list items
            foreach (Room room in rooms)
                AddGameListItem(room);
        }

        /**
        * <summary>
        * Create Game List Item prefab instance and add to games list.
        * </summary>
        */
        private void AddGameListItem(Room room)
        {
            // Show only game rooms
            // Also password protected Rooms are skipped, to make this example simpler
            // (protection would require an interface element to input the password)
            if (!room.IsGame || room.IsHidden || room.IsPasswordProtected)
                return;

            // Create game list item
            GameListItem gameListItem = Instantiate(gameListItemPrefab);
            gameListItems.Add(room.Id, gameListItem);

            // Init game list item
            gameListItem.Init(room);

            // Add listener to play button
            gameListItem.playButton.onClick.AddListener(() => OnGameItemPlayClick(room.Id));

            // Add game list item to container
            gameListItem.gameObject.transform.SetParent(gameListContent, false);
        }
        #endregion

        //----------------------------------------------------------
        // SmartFoxServer event listeners
        //----------------------------------------------------------
        #region
        private void OnRoomCreationError(BaseEvent evt)
        {
            // Show Warning Panel prefab instance
            var warningText = warningPanel.GetComponent<Metadata>();
            warningText.FindParamComponent<TextMeshProUGUI>("WarningText").text = "Room creation failed: " + (string)evt.Params["errorMessage"];
            warningPanel.SetActive(true);
        }

        private void OnRoomAdded(BaseEvent evt)
        {
            Room room = (Room)evt.Params["room"];

            // Display game list item
            AddGameListItem(room);
        }

        public void OnRoomRemoved(BaseEvent evt)
        {
            Room room = (Room)evt.Params["room"];

            // Get reference to game list item corresponding to Room
            gameListItems.TryGetValue(room.Id, out GameListItem gameListItem);

            // Remove game list item
            if (gameListItem != null)
            {
                // Remove listeners
                gameListItem.playButton.onClick.RemoveAllListeners();

                // Remove game list item from dictionary
                gameListItems.Remove(room.Id);

                // Destroy game object
                GameObject.Destroy(gameListItem.gameObject);
            }
        }

        public void OnUserCountChanged(BaseEvent evt)
        {
            Room room = (Room)evt.Params["room"];

            // Get reference to game list item corresponding to Room
            gameListItems.TryGetValue(room.Id, out GameListItem gameListItem);

            // Update game list item
            gameListItem?.SetState(room);
        }

        private void OnRoomJoin(BaseEvent evt)
        {
            // Load game scene
            SceneManager.LoadScene("Game");
        }

        private void OnRoomJoinError(BaseEvent evt)
        {
            // Show Warning Panel prefab instance
            var warningText = warningPanel.GetComponent<Metadata>();
            warningText.FindParamComponent<TextMeshProUGUI>("WarningText").text = "Room creation failed: " + (string)evt.Params["errorMessage"];
            warningPanel.SetActive(true);
        }
        #endregion
    }
}