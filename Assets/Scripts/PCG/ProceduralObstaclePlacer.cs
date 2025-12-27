using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using DG.Tweening;
using StudioByStorm.Repositories;
using StudioByStorm.Graph;
using StudioByStorm.Data;
using StudioByStorm.Obstacles.Animations;

namespace StudioByStorm.PCG {

    public class ProceduralObstaclePlacer : MonoBehaviour
    {
        [SerializeField] private GameObject obstacleContainer;
        private ObstacleDataRepository ObstacleDataRepository;
        private GraphConstructionManager GraphConstructionManager;
        private List<int> greyNodeIDs;
        public List<int> usedGreyNodeIDs;
        private List<string> placedObstacleNames;
        private static List<GameObject> Obstacles = new List<GameObject>();
        private static List<AsyncOperationHandle> ObstacleHandles = new List<AsyncOperationHandle>();

        public void DependencyInjection(ObstacleDataRepository obr, GraphConstructionManager gcm)
        {
            ObstacleDataRepository = obr;
            GraphConstructionManager = gcm;
        }

        public IEnumerator PlaceObstacles()
        {
            //Release addressables
            UnloadObstacles();

            //get the grey nodes ids
            //actually just get all ids except the first in the safe path
            greyNodeIDs = GraphConstructionManager.nodeColors.Select((n, index) => new { NodeColor = n, Index = index })
                                                             //.Where(x => x.NodeColor == NodeColor.GrayScale)
                                                             .Where(x => x.Index != GraphConstructionManager.GlobalLevelData.safePath[0])
                                                             .Select(x => x.Index)
                                                             .ToList();

            //shuffle the grey nodes
            //Shuffle shuffle = new Shuffle();
            //greyNodeIDs = shuffle.FisherYates(greyNodeIDs);

            //initialize list for nodes that end up being used by obstacles
            usedGreyNodeIDs = new List<int>();

            //initialize list for obstacles that actually get placed
            placedObstacleNames = new List<string>();

            //create the obstacle position list
            GraphConstructionManager.GlobalLevelData.obstaclePositions = new List<VectorData>();

            //create the obstacle rotation list
            GraphConstructionManager.GlobalLevelData.obstacleRotations = new List<VectorData>();

            //create the obstacle to node list
            GraphConstructionManager.GlobalLevelData.obstacleNodeIDs = new List<List<int>>();

            //iterate over the selected obstacles
            for (int i = 0; i < GraphConstructionManager.GlobalLevelData.obstacleNames.Count; i++) {

                //get the obstacle name and type
                string obstacleName = GraphConstructionManager.GlobalLevelData.obstacleNames[i];
                ObstacleType obstacleType = ObstacleDataRepository.data.Where(x => x.name == obstacleName).First().obstacleType;

                //setup the proper obstacle position placement
                switch(obstacleType) {
                    case ObstacleType.SingleNode :
                        SingleNodeObstacleTypePlacement(i);
                        break;
                    case ObstacleType.DoubleNode :
                        yield return StartCoroutine(DoubleNodeObstacleTypePlacement(i));
                        break;
                    case ObstacleType.TripleNode :
                        TripleNodeObstacleTypePlacement(i);
                        break;
                    case ObstacleType.BetweenNode :
                        BetweenNodeObstacleTypePlacement(i);
                        break;
                    case ObstacleType.OnNode :
                        OnNodeObstacleTypePlacement(i);
                        break;
                }
            }

            //update obstacle names with only the ones that were able to be placed
            GraphConstructionManager.GlobalLevelData.obstacleNames = placedObstacleNames;

            yield return null;
        }

        protected void SingleNodeObstacleTypePlacement(int obstacleNameIndex)
        {
            int greyNodeIndex = -1;

            //look for a grey node that we can place at
            for (int i = 1; i < GraphConstructionManager.GlobalLevelData.safePath.Count; i++) {
                //get the current index
                int currentGreyNodeIndex = GraphConstructionManager.GlobalLevelData.safePath[i];
                //if it's available, use it
                if (! usedGreyNodeIDs.Contains(currentGreyNodeIndex)) {
                    usedGreyNodeIDs.Add(currentGreyNodeIndex);
                    greyNodeIndex = currentGreyNodeIndex;
                    break;
                }
            }

            //if we found a spot to place the obstacle
            if (greyNodeIndex != -1) {
                //scale the position based on the original node position
                Vector3 scaled = GraphConstructionManager.nodePositions[greyNodeIndex] * 12.0f;
                //create our VectorData from it
                VectorData pos = new VectorData(scaled);
                //use random rotation
                float[] angleOffsets = new float[4]{0.0f, 180.0f, 90.0f, -90.0f};
                Vector3 randRot = new Vector3(0.0f, 0.0f, angleOffsets[Random.Range(0, angleOffsets.Length)]);
                VectorData rot = new VectorData(randRot);
                //update our level's obstacle position list
                GraphConstructionManager.GlobalLevelData.obstaclePositions.Add(pos);
                //update our level's obstacle rotation list
                GraphConstructionManager.GlobalLevelData.obstacleRotations.Add(rot);
                //update our obstacle to node list
                GraphConstructionManager.GlobalLevelData.obstacleNodeIDs.Add(new List<int>(){greyNodeIndex});
                //keep track of the obstacle name since we found a place for it
                placedObstacleNames.Add(GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex]);
            }
        }

        IEnumerator DoubleNodeObstacleTypePlacement(int obstacleNameIndex)
        {
            int firstFoundGreyNodeIndex = -1;
            int secondFoundGreyNodeIndex = -1;

            for (int i = 1; i < GraphConstructionManager.GlobalLevelData.safePath.Count; i++) {

                int firstGreyNodeIndex = GraphConstructionManager.GlobalLevelData.safePath[i]; 
                Vector3 firstGreyNodePosition = GraphConstructionManager.nodePositions[firstGreyNodeIndex];

                for (int j = 1; j < GraphConstructionManager.GlobalLevelData.safePath.Count; j++) {

                    int secondGreyNodeIndex = GraphConstructionManager.GlobalLevelData.safePath[j]; 
                    Vector3 secondGreyNodePosition = GraphConstructionManager.nodePositions[secondGreyNodeIndex];

                    //dont' compare node to itself
                    if (i == j || j != (i+1)) {
                        continue;
                    }

                    //if neither of the node indexs are being used, and the distance between the nodes is less than our threshold
                    if (! usedGreyNodeIDs.Contains(firstGreyNodeIndex) 
                        && ! usedGreyNodeIDs.Contains(secondGreyNodeIndex) 
                        && Vector3.Distance(firstGreyNodePosition, secondGreyNodePosition) <= 1.2f) {

                        //keep track of used nodes
                        usedGreyNodeIDs.Add(firstGreyNodeIndex);
                        usedGreyNodeIDs.Add(secondGreyNodeIndex);
                        firstFoundGreyNodeIndex = firstGreyNodeIndex;
                        secondFoundGreyNodeIndex = secondGreyNodeIndex;
                        break;
                    }
                }

                //if we found nodes, break out of loop
                if (firstFoundGreyNodeIndex != -1 && secondFoundGreyNodeIndex != -1) {
                    break;
                }
            }

            //if we found a spot to place the obstacle
            if (firstFoundGreyNodeIndex != -1 && secondFoundGreyNodeIndex != -1) {
                Vector3 firstScaled = GraphConstructionManager.nodePositions[firstFoundGreyNodeIndex] * 12.0f;
                Vector3 secondScaled = GraphConstructionManager.nodePositions[secondFoundGreyNodeIndex] * 12.0f;

                //get direction
                Vector3 nodeDir = (secondScaled - firstScaled).normalized;
                Vector3 perpVec = Vector3.Cross(nodeDir, Vector3.forward);
                float angle = Mathf.Atan2(perpVec.y, perpVec.x) * Mathf.Rad2Deg;
                nodeDir = new Vector3(0, 0, angle);
                
                //create centroid data
                List<float[]> centroidData = new List<float[]>();
                float[] firstScaledArray = new float[2]{firstScaled.x, firstScaled.y};
                float[] secondScaledArray = new float[2]{secondScaled.x, secondScaled.y};
                centroidData.Add(firstScaledArray);
                centroidData.Add(secondScaledArray);

                //get centroid
                float[] nodesCentroidArray = ML.Math.GetCentroid(centroidData.ToArray());
                Vector3 nodesCentroid = new Vector3(nodesCentroidArray[0], nodesCentroidArray[1], 0.0f);

                //instantiate obstacle
                string obstacleName = GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex];
                AssetReference assetRef = ObstacleDataRepository.data.Where(x => x.name == obstacleName).First().assetReference;
                AsyncOperationHandle<GameObject> loadHandle = assetRef.LoadAssetAsync<GameObject>();
                yield return loadHandle;
                GameObject go = Instantiate(loadHandle.Result, Vector3.zero, Quaternion.Euler(nodeDir), obstacleContainer.transform);

                //keep track of used addressables
                Obstacles.Add(go);
                ObstacleHandles.Add(loadHandle);

                //get the composite animation on the obstacle
                CompositeAnimation compositeAnimation = go.GetComponent<CompositeAnimation>();
                //Get all the composite animation's parts
                GameObject[] obstacleParts = compositeAnimation.animations.SelectMany<Obstacles.Animations.Animation, GameObject>(x => x.buildingBlocks).ToArray();
                //calculate an AABB on the part's positions
                Bounds partBounds = ML.Math.ComputeAABB(obstacleParts.Select(x => x.transform.position).ToList());
                //check if the animation is manually centered, if so use that center
                GameObject[] centerParts = compositeAnimation.animations.Select<Obstacles.Animations.Animation, GameObject>(x => x.centerPosition).ToArray();
                GameObject centerPart = null;
                bool foundCenterPart = false;
                for (int i = 0; i < centerParts.Length; i++) {
                    if (centerParts[i] != null) {
                        centerPart = centerParts[i];
                        foundCenterPart = true;
                        break;
                    }
                }
                Vector3 centerPositionToseUse = (centerPart != null) ? centerPart.transform.position : partBounds.center;
                //using this, we can determine the offset from the screen's center position 
                Vector3 CompositeAnimationOffset = nodesCentroid - centerPositionToseUse;
                //then we can add this offset to the composite animation's position to center it properly
                Vector3 obstacleTargetPosition = compositeAnimation.gameObject.transform.position + CompositeAnimationOffset;

                //set obstacle position
                VectorData pos = new VectorData(obstacleTargetPosition);
                VectorData rot = new VectorData(nodeDir);
                GraphConstructionManager.GlobalLevelData.obstaclePositions.Add(pos);
                GraphConstructionManager.GlobalLevelData.obstacleRotations.Add(rot);
                GraphConstructionManager.GlobalLevelData.obstacleNodeIDs.Add(new List<int>(){firstFoundGreyNodeIndex, secondFoundGreyNodeIndex});
                placedObstacleNames.Add(GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex]);

                //clean up
                Destroy(go);
            }

            yield return null;
        }

        protected void TripleNodeObstacleTypePlacement(int obstacleNameIndex)
        {

        }

        protected void BetweenNodeObstacleTypePlacement(int obstacleNameIndex)
        {
            int firstFoundGreyNodeIndex = -1;
            int secondFoundGreyNodeIndex = -1;

            for (int i = 1; i < GraphConstructionManager.GlobalLevelData.safePath.Count; i++) {

                int firstGreyNodeIndex = GraphConstructionManager.GlobalLevelData.safePath[i]; 
                Vector3 firstGreyNodePosition = GraphConstructionManager.nodePositions[firstGreyNodeIndex];

                for (int j = 1; j < GraphConstructionManager.GlobalLevelData.safePath.Count; j++) {

                    int secondGreyNodeIndex = GraphConstructionManager.GlobalLevelData.safePath[j]; 
                    Vector3 secondGreyNodePosition = GraphConstructionManager.nodePositions[secondGreyNodeIndex];

                    //dont' compare node to itself
                    if (i == j || j != (i+1)) {
                        continue;
                    }

                    //if neither of the node indexs are being used, and the distance between the nodes is less than our threshold
                    if (! usedGreyNodeIDs.Contains(firstGreyNodeIndex) 
                        && ! usedGreyNodeIDs.Contains(secondGreyNodeIndex) 
                        && Vector3.Distance(firstGreyNodePosition, secondGreyNodePosition) <= 1.2f) {

                        //keep track of used nodes
                        usedGreyNodeIDs.Add(firstGreyNodeIndex);
                        usedGreyNodeIDs.Add(secondGreyNodeIndex);
                        firstFoundGreyNodeIndex = firstGreyNodeIndex;
                        secondFoundGreyNodeIndex = secondGreyNodeIndex;
                        break;
                    }
                }

                //if we found nodes, break out of loop
                if (firstFoundGreyNodeIndex != -1 && secondFoundGreyNodeIndex != -1) {
                    break;
                }
            }

            //if we found a spot to place the obstacle
            if (firstFoundGreyNodeIndex != -1 && secondFoundGreyNodeIndex != -1) {
                //get node positions
                Vector3 firstScaled = GraphConstructionManager.nodePositions[firstFoundGreyNodeIndex] * 12.0f;
                Vector3 secondScaled = GraphConstructionManager.nodePositions[secondFoundGreyNodeIndex] * 12.0f;

                //get direction
                Vector3 nodeDir = (firstScaled - secondScaled).normalized;
                Vector3 perpVec = Vector3.Cross(nodeDir, Vector3.forward);
                float angle = Mathf.Atan2(perpVec.y, perpVec.x) * Mathf.Rad2Deg;
                nodeDir = new Vector3(0, 0, angle);

                //create centroid data
                List<float[]> centroidData = new List<float[]>();
                float[] firstScaledArray = new float[2]{firstScaled.x, firstScaled.y};
                float[] secondScaledArray = new float[2]{secondScaled.x, secondScaled.y};
                centroidData.Add(firstScaledArray);
                centroidData.Add(secondScaledArray);

                //get centroid
                float[] nodesCentroidArray = ML.Math.GetCentroid(centroidData.ToArray());
                Vector3 nodesCentroid = new Vector3(nodesCentroidArray[0], nodesCentroidArray[1], 0.0f);

                //set obstacle position
                VectorData pos = new VectorData(nodesCentroid);
                VectorData rot = new VectorData(nodeDir);
                GraphConstructionManager.GlobalLevelData.obstaclePositions.Add(pos);
                GraphConstructionManager.GlobalLevelData.obstacleRotations.Add(rot);
                GraphConstructionManager.GlobalLevelData.obstacleNodeIDs.Add(new List<int>(){firstFoundGreyNodeIndex, secondFoundGreyNodeIndex});
                placedObstacleNames.Add(GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex]);
            }
        }

        protected void OnNodeObstacleTypePlacement(int obstacleNameIndex)
        {
            int firstFoundGreyNodeIndex = -1;
            int secondFoundGreyNodeIndex = -1;
            int thirdFoundGreyNodeIndex = -1;

            for (int i = 1; i < GraphConstructionManager.GlobalLevelData.safePath.Count; i++) {

                int firstGreyNodeIndex = GraphConstructionManager.GlobalLevelData.safePath[i]; 
                Vector3 firstGreyNodePosition = GraphConstructionManager.nodePositions[firstGreyNodeIndex];


                //use previous node ie one before
                int secondGreyNodeIndex = GraphConstructionManager.GlobalLevelData.safePath[i - 1]; 
                Vector3 secondGreyNodePosition = GraphConstructionManager.nodePositions[secondGreyNodeIndex];

                bool hasThirdNode = (i+1) < GraphConstructionManager.GlobalLevelData.safePath.Count;

                bool isDirectionOkay = true;

                if (!hasThirdNode) {
                    isDirectionOkay = true;
                //check next node too
                } else {
                    int thirdGreyNodeIndex = GraphConstructionManager.GlobalLevelData.safePath[i + 1]; 

                    //get node positions
                    Vector3 firstScaled = GraphConstructionManager.nodePositions[firstGreyNodeIndex] * 12.0f;
                    Vector3 secondScaled = GraphConstructionManager.nodePositions[secondGreyNodeIndex] * 12.0f;
                    Vector3 thirdScaled = GraphConstructionManager.nodePositions[thirdGreyNodeIndex] * 12.0f;

                    //get direction between 1st and 2nd. First is current, second is previous
                    Vector3 firstNodeDir = (firstScaled - secondScaled).normalized;
                    //get direction between 3rd and 1st. Third is next current, and then first would be previous relative to that
                    Vector3 thirdNodeDir = (thirdScaled - firstScaled).normalized;
                    
                    if (firstNodeDir == thirdNodeDir) {
                        isDirectionOkay = true;
                    } else {
                        isDirectionOkay = false;
                    }
                }

                //check to see if its colored
                NodeColor firstNodeColor = GraphConstructionManager.nodeColors[firstGreyNodeIndex];
                bool isColored = (firstNodeColor != NodeColor.GrayScale);

                //check to see if previous color is the same color
                bool isPreviousParentSameColor = false;
                
                if (isColored) {
                    if (i == 1) {
                        isPreviousParentSameColor = true;
                    } else {
                        int startIndex = i-1;
                        for (int k = startIndex ; k > 0; k--) {
                            NodeColor currentNodeColor = GraphConstructionManager.nodeColors[k];
                            bool isCurrentNodeColored = (currentNodeColor != NodeColor.GrayScale);
                            //breakout once previous color is found
                            if (isCurrentNodeColored) {
                                isPreviousParentSameColor = (currentNodeColor == firstNodeColor);
                                break;
                            }
                        }
                    }
                }
                //color coordination ie navigation solution to coloring issues with on node obstacles
                //ie used to cause color change then instant collision
                bool isColorCoordinationOkay = (!isColored || (isColored && isPreviousParentSameColor));

                //if neither of the node indexs are being used, and the distance between the nodes is less than our threshold
                if (! usedGreyNodeIDs.Contains(firstGreyNodeIndex) && isDirectionOkay && isColorCoordinationOkay) {
                    //keep track of used nodes
                    usedGreyNodeIDs.Add(firstGreyNodeIndex);
                    firstFoundGreyNodeIndex = firstGreyNodeIndex;
                    secondFoundGreyNodeIndex = secondGreyNodeIndex;
                    break;
                }
            }
            
            //if we found a spot to place the obstacle
            if (firstFoundGreyNodeIndex != -1 && secondFoundGreyNodeIndex != -1) {
                //get node positions
                Vector3 firstScaled = GraphConstructionManager.nodePositions[firstFoundGreyNodeIndex] * 12.0f;
                Vector3 secondScaled = GraphConstructionManager.nodePositions[secondFoundGreyNodeIndex] * 12.0f;

                //get direction
                Vector3 nodeDir = (firstScaled - secondScaled).normalized;
                Vector3 perpVec = Vector3.Cross(nodeDir, Vector3.forward);
                float angle = Mathf.Atan2(perpVec.y, perpVec.x) * Mathf.Rad2Deg;
                nodeDir = new Vector3(0, 0, angle);

                //use node position as centroid
                Vector3 nodesCentroid = new Vector3(firstScaled.x, firstScaled.y, 0.0f);

                //set obstacle position
                VectorData pos = new VectorData(nodesCentroid);
                VectorData rot = new VectorData(nodeDir);
                GraphConstructionManager.GlobalLevelData.obstaclePositions.Add(pos);
                GraphConstructionManager.GlobalLevelData.obstacleRotations.Add(rot);
                GraphConstructionManager.GlobalLevelData.obstacleNodeIDs.Add(new List<int>(){firstFoundGreyNodeIndex});
                placedObstacleNames.Add(GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex]);
            }
        }

        protected void UnloadObstacles()
        {
            for (int i = 0; i < Obstacles.Count; i++) {
                if (Obstacles[i] != null) {
                    Destroy(Obstacles[i]);
                }
            }
            Obstacles = new List<GameObject>();

            for (int j = 0; j < ObstacleHandles.Count; j++) {
                Addressables.Release(ObstacleHandles[j]);
            }
            ObstacleHandles = new List<AsyncOperationHandle>();
        }
    }

}