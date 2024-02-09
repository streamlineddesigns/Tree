using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using StudioByStorm.Data;

namespace StudioByStorm {

    public class ProgressManager
    {
        private ProgressData ProgressData;
        private string LevelSaveFilePath;

        public ProgressManager()
        {
            string dir = Application.persistentDataPath;
            LevelSaveFilePath = (dir + "/progress.dat");
            Load();
        }

        public void Load()
        {
            ProgressData = Deserialize<ProgressData>(LevelSaveFilePath);
        }

        public void Save()
        {
            Serialize<ProgressData>(ProgressData, LevelSaveFilePath);
        }

        //Key is string(ChapterID + "-" + LevelID)
        public int GetLevelProgress(string k)
        {
            if (! ProgressData.levelProgress.ContainsKey(k)) {
                return 0;
            }

            return ProgressData.levelProgress[k];
        }

        //Key is string(ChapterID + "-" + CutSceneID)
        public bool GetCutSceneProgress(string k)
        {
            if (! ProgressData.cutSceneProgress.ContainsKey(k)) {
                return false;
            }

            return true;
        }

        //Key is int(ChapterID)
        public int GetUnlockedLevelProgress(int k)
        {
            if (! ProgressData.unlockedLevelProgress.ContainsKey(k)) {
                return -1;
            }

            return ProgressData.unlockedLevelProgress[k];
        }

        //Key is int(ChapterID)
        public int GetUnlockedCutSceneProgress(int k)
        {
            if (! ProgressData.unlockedCutSceneProgress.ContainsKey(k)) {
                return -1;
            }

            return ProgressData.unlockedCutSceneProgress[k];
        }

        public int GetTutorialProgress()
        {
            return ProgressData.tutorialProgress;
        }

        public void UpdateLevel(string k, int v)
        {
            if (ProgressData.levelProgress.ContainsKey(k)) {
                ProgressData.levelProgress[k] = v;
            } else {
                ProgressData.levelProgress.Add(k, v);
            }
        }

        public void UpdateCutScene(string k, bool v)
        {
            if (ProgressData.cutSceneProgress.ContainsKey(k)) {
                ProgressData.cutSceneProgress[k] = v;
            } else {
                ProgressData.cutSceneProgress.Add(k, v);
            }
        }

        public void UpdateUnlockedLevel(int k, int v)
        {
            if (ProgressData.unlockedLevelProgress.ContainsKey(k)) {
                ProgressData.unlockedLevelProgress[k] = v;
            } else {
                ProgressData.unlockedLevelProgress.Add(k, v);
            }
        }

        public void UpdateUnlockedCutScene(int k, int v)
        {
            if (ProgressData.unlockedCutSceneProgress.ContainsKey(k)) {
                ProgressData.unlockedCutSceneProgress[k] = v;
            } else {
                ProgressData.unlockedCutSceneProgress.Add(k, v);
            }
        }

        public void UpdateTutorial(int k)
        {
            ProgressData.tutorialProgress = k;
        }

        private void Serialize<T>(T obj, string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(filePath, FileMode.Create);

            try
            {
                formatter.Serialize(stream, obj);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Serialization failed! " + e.Message);
            }
            finally
            {
                stream.Close();
            }
        }

        private T Deserialize<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                Debug.Log("Serialization file not found at " + filePath);
                return new T();
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(filePath, FileMode.Open);
            T obj = default(T);

            try
            {
                obj = (T)formatter.Deserialize(stream);
            }
            catch (SerializationException e)
            {
                Debug.LogError("Deserialization failed! " + e.Message);
            }
            finally
            {
                stream.Close();
            }

            return obj;
        }

    }

}