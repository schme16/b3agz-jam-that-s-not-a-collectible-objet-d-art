using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveManagerScript : MonoBehaviour {




	public void ResetAllSaves() {

		//Reset the art
		SaveArtList(new List<GameController.Art>(), "collectedPortraitArt");
		SaveArtList(new List<GameController.Art>(), "collectedSquareArt");

		//Reset the flags
		SaveFlagsList(new GameController.Flags(), "flags");

		//Reset the messages
		SaveMessagesList(new List<int>(), "messages");
	}







	public void SaveArtList(List<GameController.Art> artList, string key) {
		string json = JsonUtility.ToJson(new ArtListWrapper(artList));
		PlayerPrefs.SetString(key, json);
		PlayerPrefs.Save();
	}

	public List<GameController.Art> LoadArtList(string key) {
		if (!PlayerPrefs.HasKey(key)) return new List<GameController.Art>();

		string json = PlayerPrefs.GetString(key);
		ArtListWrapper wrapper = JsonUtility.FromJson<ArtListWrapper>(json);
		return wrapper.artworks ?? new List<GameController.Art>();
	}

	[Serializable]
	private class ArtListWrapper {
		public List<GameController.Art> artworks;
		public ArtListWrapper(List<GameController.Art> artList) { artworks = artList; }
	}








	public List<GameController.Sale> LoadPurchases() {

		var purchases = LoadPurchaseList("purchases");
		if (purchases is null) {
			purchases = new List<GameController.Sale>();
			SavePurchaseList(purchases, "purchases");
		}


		return purchases;
	}

	public List<GameController.Sale> SavePurchases(List<GameController.Sale> purchases) {
		SavePurchaseList(purchases, "purchases");
		return LoadPurchases();
	}

	public void SavePurchaseList(List<GameController.Sale> artList, string key) {
		var json = JsonUtility.ToJson(new PurchaseListWrapper(artList));
		PlayerPrefs.SetString(key, json);
		PlayerPrefs.Save();
	}

	public List<GameController.Sale> LoadPurchaseList(string key) {
		if (!PlayerPrefs.HasKey(key)) return new List<GameController.Sale>();

		var json = PlayerPrefs.GetString(key);
		var wrapper = JsonUtility.FromJson<PurchaseListWrapper>(json);
		return wrapper.artworks ?? new List<GameController.Sale>();
	}

	[Serializable]
	private class PurchaseListWrapper {
		public List<GameController.Sale> artworks;
		public PurchaseListWrapper(List<GameController.Sale> artList) { artworks = artList; }
	}






	public GameController.Flags LoadFlags() {

		return LoadFlagsList("flags");
	}

	public void SaveFlags(GameController.Flags flags) {
		SaveFlagsList(flags, "flags");
		LoadFlags();
	}

	public void SaveFlagsList(GameController.Flags flagsList, string key) {

		flagsList.hasBeenLoaded = true;
		var json = JsonUtility.ToJson(new FlagsListWrapper(flagsList));
		PlayerPrefs.SetString(key, json);
		PlayerPrefs.Save();
	}

	public GameController.Flags LoadFlagsList(string key) {
		if (!PlayerPrefs.HasKey(key)) return new GameController.Flags();

		var json = PlayerPrefs.GetString(key);
		var wrapper = JsonUtility.FromJson<FlagsListWrapper>(json);
		return wrapper.flags.hasBeenLoaded ? wrapper.flags : new GameController.Flags();
	}

	[Serializable]
	private class FlagsListWrapper {
		public GameController.Flags flags;
		public FlagsListWrapper(GameController.Flags flagsList) { flags = flagsList; }
	}






	/*Answering machine*/




	public List<int> LoadMessages() {

		var pendingMessages = LoadMessagesList("messages");

		if (pendingMessages is null || pendingMessages.Count == 0) {
			pendingMessages = new List<int>();
			SaveMessagesList(pendingMessages, "messages");
		}

		return pendingMessages;

	}

	public void SaveMessages(List<int> pendingMessages) {
		SaveMessagesList(pendingMessages, "messages");
		LoadMessages();
	}

	public void SaveMessagesList(List<int> messagesList, string key) {
		var json = JsonUtility.ToJson(new MessagesListWrapper(messagesList));
		PlayerPrefs.SetString(key, json);
		PlayerPrefs.Save();
	}

	public List<int> LoadMessagesList(string key) {
		if (!PlayerPrefs.HasKey(key)) return new List<int>();

		var json = PlayerPrefs.GetString(key);
		var wrapper = JsonUtility.FromJson<MessagesListWrapper>(json);
		return wrapper.messages ?? new List<int>();
	}

	[Serializable]
	private class MessagesListWrapper {
		public List<int> messages;
		public MessagesListWrapper(List<int> messagesList) { messages = messagesList; }
	}




}
