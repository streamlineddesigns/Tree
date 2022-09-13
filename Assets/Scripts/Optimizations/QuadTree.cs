using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioByStorm.Optimizations {

    /*
     * Allows you to reduce the time complexity of NN search from O(n) to O(log(n))
     * It does this by partitioning a search space into quadrants that allow objects to be placed inside of them
     * That is, until a predetermined threshold of objects have been placed
     * After which, the quadrant will recursively subdivide itself into further sub-quadrants
     * Then, rather than check a list of objects distance's to a search location to determine nearby objects to said search location
     * We can look up a search location's quadrant in the quad tree, then ask the quad tree for the objects from that quadrant
     * Hence the reduction in time complexity
     */
    public class QuadTree
    {
        protected int maxObjectCount;
        private readonly List<GameObject> storedObjects;
        protected Rect bounds;
        private readonly QuadTree[] cells;

        /*
         * @param  int maxSize is the max amount of objects in a quadrant before it splits
         * @param Rect newBounds is the bounding volume in which the quad tree exists
         */
        public QuadTree(int maxSize, Rect newBounds){
            bounds = newBounds;
            maxObjectCount = maxSize;
            cells = new QuadTree[4];
            storedObjects = new List<GameObject>(maxSize);
        }

        
        public void Insert(GameObject objectToInsert)
        {
            if(cells[0] != null)
            {
                int iCell = GetInsertCell(Get2DPosition(objectToInsert));
                if(iCell >= 0)
                {
                    cells[iCell].Insert(objectToInsert);
                }
                else
                {
                    Debug.Log("gameObject is out of the insert rect" + Get2DPosition(objectToInsert) + objectToInsert);
                }
                return;
            }

            storedObjects.Add(objectToInsert);

            //Objects exceed the maximum count
            if(storedObjects.Count > maxObjectCount)
            {
                //Split the quad into 4 sections
                if(cells[0] == null)
                {
                    float subWidth = (bounds.width / 2f);
                    float subHeight = (bounds.height / 2f);
                    
                    //NorthEast
                    cells[0] = new QuadTree(maxObjectCount,new Rect(bounds.x + subWidth, bounds.y, subWidth, subHeight));
                    //northWest
                    cells[1] = new QuadTree(maxObjectCount,new Rect(bounds.x, bounds.y, subWidth, subHeight));
                    //southWest
                    cells[2] = new QuadTree(maxObjectCount,new Rect(bounds.x, bounds.y + subHeight, subWidth, subHeight));
                    //southEast
                    cells[3] = new QuadTree(maxObjectCount,new Rect(bounds.x + subWidth, bounds.y + subHeight, subWidth, subHeight));
                }
                //Reallocate this quads objects into its children
                for(int i = storedObjects.Count-1; i >= 0; --i)
                {
                    GameObject storedObj = storedObjects[i];
                    int index = GetInsertCell(Get2DPosition(storedObj));
                    if (index == -1 || cells[index] == null) {
                        return;
                    }
                    cells[index].Insert(storedObj);
                    storedObjects.RemoveAt(i);
                }
            }
        }
        
        public void Remove(GameObject objectToRemove)
        {
            if(ContainsLocation(Get2DPosition(objectToRemove)))
            {
                storedObjects.Remove(objectToRemove);
                if(cells[0] != null)
                {
                    for(int i=0; i < 4; i++)
                    {
                        cells[i].Remove(objectToRemove);
                    }
                }
            }
        }

        public void Clear()
        {
            storedObjects.Clear();
            
            for(int i  = 0; i < cells.Length; i++)
            {
                if(cells[i] != null)
                {
                    cells[i].Clear();
                    cells[i] = null;
                }
            }
        }

        private List<GameObject> RetrieveObjectsInArea(Rect area)
        {
            if(RectOverlap(bounds,area)){
                List<GameObject> returnedObjects = new List<GameObject>();
                for(int i=0; i < storedObjects.Count; ++i)
                {
                    if(area.Contains(Get2DPosition(storedObjects[i])))
                    {
                        returnedObjects.Add(storedObjects[i]);
                    }
                }
                if(cells[0] != null){
                    for(int i = 0; i < 4; i++)
                    {
                        List<GameObject> cellObjects = cells[i].RetrieveObjectsInArea(area);
                        if(cellObjects != null)
                        {
                            returnedObjects.AddRange(cellObjects);
                        }
                    }
                }
                return returnedObjects;
            }
            return null;
        }

        public GameObject FindClosestGameObject(Vector2 position, float maxDistance)
        {
            Rect viewRect = new Rect(position.x - maxDistance/2, position.y - maxDistance/2, maxDistance, maxDistance);
            
            List<GameObject> closestObjects = RetrieveObjectsInArea(viewRect);
            if (closestObjects == null || closestObjects.Count <= 0)
            {
                return null;
            }
            
            GameObject closestObject = null;
            float closestDist = Mathf.Infinity;

            foreach (GameObject obj in closestObjects)
            {
                float newDist = Vector2.Distance(Get2DPosition(obj), position);
                if (newDist < closestDist)
                {
                    closestDist = newDist;
                    closestObject = obj;
                }
            }
            
            return closestObject;
        }

        protected bool ContainsLocation(Vector2 location)
        {
            return bounds.Contains(location);
        }
        
        private int GetInsertCell(Vector2 location)
        {
            for(int i=0; i < 4; i++){
                if(cells[i].ContainsLocation(location))
                {
                    return i;
                }
            }
            return -1;
        }
        private static bool ValueInRange(float value, float min, float max)
        { return (value >= min) && (value <= max); }

        protected static bool RectOverlap(Rect A, Rect B)
        {
            bool xOverlap = ValueInRange(A.x, B.x, B.x + B.width) ||
                            ValueInRange(B.x, A.x, A.x + A.width);

            bool yOverlap = ValueInRange(A.y, B.y, B.y + B.height) ||
                            ValueInRange(B.y, A.y, A.y + A.height);

            return xOverlap && yOverlap;
        }

        private static Vector2 Get2DPosition(GameObject obj)
        {
            Vector3 pos = obj.transform.position;
            return V3ToV2(pos);
        }
        
        protected static Vector2 V3ToV2(Vector3 vector3)
        {
            return new Vector2(vector3.x, vector3.z);
        }

        public void DrawDebug(){
            Gizmos.DrawLine(new Vector3(bounds.x,0, bounds.y),new Vector3(bounds.x,0,bounds.y+ bounds.height));
            Gizmos.DrawLine(new Vector3(bounds.x,0, bounds.y),new Vector3(bounds.x+bounds.width,0,bounds.y));
            Gizmos.DrawLine(new Vector3(bounds.x+bounds.width,0, bounds.y),new Vector3(bounds.x+bounds.width,0,bounds.y+ bounds.height));
            Gizmos.DrawLine(new Vector3(bounds.x,0, bounds.y+bounds.height),new Vector3(bounds.x+bounds.width,0,bounds.y+bounds.height));
            if(cells[0] != null){			
                for(int i  = 0; i < cells.Length; i++)
                {
                    if(cells[i] != null)
                    {
                        cells[i].DrawDebug();
                    }
                }
            }
        }
    }

}