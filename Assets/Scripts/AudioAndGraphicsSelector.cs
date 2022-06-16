using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Text;
using System.Linq;
public class AudioAndGraphicsSelector : MonoBehaviour
{

    public GameObject optionsMenu;//eventually should become a prefab that's managed on creation
    public Dropdown testDrop;

    private static int soundsThisFrame = 0;
    private static int soundsPerFrameCap = 10;

    public static Sprite errorSprite;
    public static AudioClip errorSound;

    private static string spritePath = "GraphicsPacks/DefaultImages";//the currently selected graphics pack
    private static string audioPath = "AudioPacks/DefaultAudio";
    private static string defaultSprites = "GraphicsPacks/DefaultImages";//the default pack used if no sprite is found
    private static string defaultAudio = "AudioPacks/DefaultAudio";
    private static AudioSource source;
    private static AudioSource music;

    public static Sprite GetSprite(string name)
    {
        Sprite toLoad = Resources.Load<Sprite>(spritePath + "/" + name);//uses corresponding texture if it's in the pack
        if (toLoad != null)
            return toLoad;
        toLoad = Resources.Load<Sprite>(defaultSprites + "/" + name);//otherwise uses default corresponding texture
        if (toLoad != null)
            return toLoad;
        Debug.LogWarning("Missing texture. Did you misspell \"" + name + "\"?");
        return errorSprite; //if all else fails, return White Pawn
    }

    public static void PlaySound(string name)
    {
        AudioClip toLoad = Resources.Load<AudioClip>(audioPath + "/" + name);//uses corresponding sound if it's in the pack
        if (toLoad != null)
        {
            PlayThisSound(toLoad);
            return;
        }
        toLoad = Resources.Load<AudioClip>("DefaultAudio/" + name);//otherwise uses default pack's sound
        if (toLoad != null)
        {
            PlayThisSound(toLoad);
            return;
        }
        Debug.LogWarning("Missing sound. Did you misspell \"" + name + "\"?");
        PlayThisSound(toLoad);
        return; //if all else fails, return a thud sound
    }
    public static void PlayThisSound(AudioClip sound)
    {
        if (soundsThisFrame < soundsPerFrameCap)
        {
            soundsThisFrame++;
            source.PlayOneShot(sound);
        }
    }
    public static void SetSpritePath(string path)//yes, I know it's currently public and you can just change it like that, but eventually this may tell objects to reload their sprites
    {
        spritePath = path;
    }
    public static void SetAudioPath(string path)
    {
        audioPath = path;
    }


    private void Awake()
    {
        errorSprite = Resources.Load<Sprite>(defaultSprites + "/White Pawn");
        errorSound = Resources.Load<AudioClip>(defaultAudio + "/Error Sound");




        var temp = gameObject.GetComponents<AudioSource>();
        if (temp == null)
        {
            Debug.LogError("Can't find audio");
            return;
        }
        music = temp[0];
        source = temp[1];
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        soundsThisFrame = 0;
        //eventually it should also look in Application.persistentDataPath and treat that the same as the Resources it loads, so you can add your own music/graphics
        if (optionsMenu != null)
        {

        }
        /*if(SaveLoad<Directory>.Load("","")!=null)
        {
            SteamLobbyChess.ToScreen("Found");
        }
        else
            SteamLobbyChess.ToScreen("Can't find");*/

    }
    /// <summary>
    /// Saves, loads and deletes all data in the game. As implemented by TEEBQNE, and modified a bit.
    /// </summary>
    public static class SaveLoad<T>
    {
        /// <summary>
        /// Save data to a file (overwrite completely)
        /// </summary>
        public static void Save(T data, string folder, string file)
        {
            // get the data path of this save data
            string dataPath = GetFilePath(folder, file);

            string jsonData = JsonUtility.ToJson(data, true);
            byte[] byteData;

            byteData = Encoding.ASCII.GetBytes(jsonData);

            // create the file in the path if it doesn't exist
            // if the file path or name does not exist, return the default SO
            if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dataPath));
            }

            // attempt to save here data
            try
            {
                // save datahere
                File.WriteAllBytes(dataPath, byteData);
                Debug.Log("Save data to: " + dataPath);
            }
            catch (Exception e)
            {
                // write out error here
                Debug.LogError("Failed to save data to: " + dataPath);
                Debug.LogError("Error " + e.Message);
            }
        }

        /// <summary>
        /// Load all data at a specified file and folder location
        /// </summary>
        public static T Load(string folder, string file)
        {
            // get the data path of this save data
            string dataPath = GetFilePath(folder, file);

            // if the file path or name does not exist, return the default SO
            if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
            {
                Debug.LogWarning("File or path does not exist! " + dataPath);
                return default(T);
            }

            // load in the save data as byte array
            byte[] jsonDataAsBytes = null;

            try
            {
                jsonDataAsBytes = File.ReadAllBytes(dataPath);
                Debug.Log("<color=green>Loaded all data from: </color>" + dataPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to load data from: " + dataPath);
                Debug.LogWarning("Error: " + e.Message);
                return default(T);
            }

            if (jsonDataAsBytes == null)
                return default(T);

            // convert the byte array to json
            string jsonData;

            // convert the byte array to json
            jsonData = Encoding.ASCII.GetString(jsonDataAsBytes);

            // convert to the specified object type
            T returnedData = JsonUtility.FromJson<T>(jsonData);

            // return the casted json object to use
            return (T)Convert.ChangeType(returnedData, typeof(T));
        }

        /// <summary>
        /// Create file path for where a file is stored on the specific platform given a folder name and file name
        /// </summary>
        private static string GetFilePath(string FolderName, string FileName = "")
        {
            string filePath;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // mac
        filePath = Path.Combine(Application.streamingAssetsPath, ("data/" + FolderName));

        if (FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".txt"));
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // windows
            filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

            if (FileName != "")
                filePath = Path.Combine(filePath, (FileName + ".txt"));
#elif UNITY_ANDROID
        // android
        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".txt"));
#elif UNITY_IOS
        // ios
        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".txt"));
#endif
            return filePath;
        }
    }
}
