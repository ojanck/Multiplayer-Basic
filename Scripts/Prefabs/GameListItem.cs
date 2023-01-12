using System;
using UnityEngine;
using UnityEngine.UI;
using Sfs2X.Entities;
using TMPro;

/**
 * <summary>
 * Script attached to Game List Item prefab.
 * </summary>
 */
public class GameListItem : MonoBehaviour
{
	public Button playButton;
	public Button watchButton;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI detailsText;

	public int roomId;

	/**
	* <summary>
	* Initialize the prefab instance.
 	* </summary>
	*/
	public void Init(Room room)
	{
		nameText.text = room.Name;
		roomId = room.Id;

		SetState(room);
	}

	/**
 	* <summary>
	* Update prefab instance based on the corresponding Room state.
 	* </summary>
	*/
	public void SetState(Room room)
	{
		int playerSlots = room.MaxUsers - room.UserCount;
		int spectatorSlots = room.MaxSpectators - room.SpectatorCount;

		// Set player count and spectator count in game list item
		detailsText.text = String.Format("Player slots: {0}\nSpectator slots: {1}", playerSlots, spectatorSlots);

		// Enable/disable game play button
		playButton.interactable = playerSlots > 0;

		// Enable/disable game watch button
		watchButton.interactable = spectatorSlots > 0;
	}
}
