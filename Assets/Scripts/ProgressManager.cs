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

        public int GetLevelProgress(string k)
        {
            if (! ProgressData.levelProgress.ContainsKey(k)) {
                return 0;
            }

            return ProgressData.levelProgress[k];
        }

        public bool GetCutSceneProgress(string k)
        {
            if (! ProgressData.cutSceneProgress.ContainsKey(k)) {
                return false;
            }

            return true;
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