using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Optimizations {
    
    /*
     * Allows any type to be placed inside a SpatialHash
     */
    [System.Serializable]
    public class HashData
    {
        public Vector2 cellID;
        public GameObject gameObject;
        public object data;

        public HashData(GameObject go, object obj)
        {
            gameObject = go;
            data = obj;
        }

        public HashData(GameObject go)
        {
            gameObject = go;
        }

        public T GetData<T>()
        {
            return (T) data;
        }
        
        public Vector2 GetPosition() {
            return (gameObject != null) ? (Vector2) gameObject.transform.position : Vector2.zero;
        }
    }

    /*
     * Allows you to reduce the time complexity of NN search from O(n) to O(1)
     * It does this by flattening a 2D space into a 1D hash table making spatial queries non Euclidean related, and just index retrievals
     * Add objects to the spatial hash in start by calling: AddObject(T hashData)
     * Update their positions by calling : UpdateObject(T hashData)
     * Get the cell ID for any specified object by calling : int GetCellIDForObj(GameObject obj)
     * Get the nearest objects in the Spatial Hash by calling : List<T> GetNearby(int cellID)
     */
	public class SpatialHash<T> where T : HashData
    {
		private Dictionary<Vector2, List<T>> cells;
		private int cellSize;

        public SpatialHash(int size)
	    {
            cells = new Dictionary<Vector2, List<T>>();
			cellSize = size;
		}

		public void AddObject(T hashData)
		{
			Vector2 cellID = GetCellIDForObj(hashData);
			Insert(cellID, hashData);
		}

		public void UpdateObject(T hashData)
		{
			Vector2 previousCellID = hashData.cellID;
			Vector2 cellID = GetCellIDForObj(hashData);

			if (previousCellID != cellID) {
                if (cells.ContainsKey(previousCellID)) {
                    cells[previousCellID].Remove(hashData);
                }
				Insert(cellID, hashData);
			}
		}

		public List<T> GetNearby(Vector2 cellID)
		{
            if (cells.ContainsKey(cellID)) {
                //return cells[cellID];
            }

            List<T> nearby = new List<T>();
            List<Vector2> nearbyCellIDList = new List<Vector2>(){ new Vector2(cellID.x, cellID.y),//current
                                                                  new Vector2(cellID.x + 1, cellID.y),//right
															      new Vector2(cellID.x - 1, cellID.y),//left 
															      new Vector2(cellID.x, cellID.y + 1),//up
															      new Vector2(cellID.x, cellID.y - 1),//down 
															      new Vector2(cellID.x + 1, cellID.y + 1),//upper right diag 
															      new Vector2(cellID.x + 1, cellID.y - 1),//lower right diag
															      new Vector2(cellID.x - 1, cellID.y - 1), //bottom left diag
															      new Vector2(cellID.x - 1, cellID.y + 1) };//upper left diag
			
			for (int i = 0; i < nearbyCellIDList.Count; i++) {
				if (cells.ContainsKey(nearbyCellIDList[i])) {
					nearby.AddRange(cells[nearbyCellIDList[i]]);
				}
			}

			return nearby;
		}

		private void Insert(Vector2 cellID, T hashData)
		{
			if (! cells.ContainsKey(cellID)) {
				cells.Add(cellID, new List<T>());
			}

			cells[cellID].Add(hashData);
			hashData.cellID = cellID;
		}

		public Vector2 GetCellIDForObj(T hashData)
		{
            return new Vector2((int)(hashData.GetPosition().x  / cellSize), (int)(hashData.GetPosition().y / cellSize));
		}

        public Vector2 GetCellIDForObj(GameObject go)
		{
            return new Vector2((int)(go.transform.position.x / cellSize), (int)(go.transform.position.y / cellSize));
		}

        public Vector2 GetCellIDForPos(Vector2 pos)
		{
            return new Vector2((int)(pos.x / cellSize), (int)(pos.y / cellSize));
		}
		
	}

}