using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Helpers;

namespace StudioByStorm.Optimizations {

	public class SpatialHashManager : MonoBehaviour
    {
        public SpatialHash<HashData> SpatialHash;
        public HashData[] HashData = new HashData[0];
        public HashData PlayerHashData;
        public GameObject nodeParent;
        public List<Node> Nodes;
        public int iCollidersToEnableOverride = 10;
        public int cellSizeOverride = 10;
        private int cellSize = 10;
        private int iCollidersToEnable = 10;
        
        void Update()
        {
            cellSize = cellSizeOverride;
            iCollidersToEnable = iCollidersToEnableOverride;

            GenerateSpatialHash();
            GenerateHashData();
            PopulateSpatialHash();
            UpdateSpatialHash();
        }

        protected void GenerateSpatialHash()
        {
            SpatialHash = new SpatialHash<HashData>(cellSize);
        }

        protected void GenerateHashData()
        {
            //get list of nodes contained inside the parent
            Nodes = GameObjectHelper.GetComponentsOfType<Node>(nodeParent, true);

            //create a list of hash data objects
            List<HashData> hashData  = new List<HashData>();

            //iterate over nodes and create hash data object from them
            for (int i = 0; i < Nodes.Count; i++) {
                HashData data = new HashData(Nodes[i].gameObject, Nodes[i]);
                hashData.Add(data);
            }

            HashData = hashData.ToArray();
        }

        protected void PopulateSpatialHash()
        {
            //add agent hash data search object
            SpatialHash.AddObject(PlayerHashData);

            //add hash data object
            for(int i = 0; i < HashData.Length; i++) {
                SpatialHash.AddObject(HashData[i]);
            }
        } 

        protected void UpdateSpatialHash()
        {
            //update all the obstacles in the spatial hash
            for(int i = 0; i < HashData.Length; i++) {
                SpatialHash.UpdateObject(HashData[i]);
            }

            //update the agent in the spatial hash
            if (SpatialHash != null && PlayerHashData != null) {
                SpatialHash.UpdateObject(PlayerHashData);
            }
            
        }

        public void SetPlayerHashData(HashData hd)
        {
            PlayerHashData = hd;
        }
	}

}