using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class Session {

	public static List<Game> savedGames = new List<Game>();
			
	public static void Save() {
        Load();
		Session.savedGames.Add(Game.current);
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (Application.persistentDataPath + "/savedGame.djinn");
		Debug.Log (Session.savedGames);
		bf.Serialize(file, Session.savedGames);
		file.Close();
	}	
	
	public static void Load() {
        Debug.Log(Application.persistentDataPath);
        if (File.Exists(Application.persistentDataPath + "/savedGame.djinn")) {
            BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/savedGame.djinn", FileMode.Open);
			Session.savedGames = (List<Game>)bf.Deserialize(file);
			file.Close();
		}
	}
}
