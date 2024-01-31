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
        public List<int> usedGreyNodeIDs = new List<int>();
        private List<string> placedObstacleNames = new List<string>();

        public void DependencyInjection(ObstacleDataRepository obr, GraphConstructionManager gcm)
        {
            ObstacleDataRepository = obr;
            GraphConstructionManager = gcm;
        }

        public IEnumerator PlaceObstacles()
        {
            //get the grey nodes ids
            greyNodeIDs = GraphConstructionManager.nodeColors.Select((n, index) => new { NodeColor = n, Index = index })
                                                             .Where(x => x.NodeColor == NodeColor.GrayScale)
                                                             .Select(x => x.Index)
                                                             .ToList();

            //shuffle the grey nodes
            Shuffle shuffle = new Shuffle();
            greyNodeIDs = shuffle.FisherYates(greyNodeIDs);

            //create the obstacle position list
            GraphConstructionManager.GlobalLevelData.obstaclePositions = new List<VectorData>();

            //create the obstacle rotation list
            GraphConstructionManager.GlobalLevelData.obstacleRotations = new List<VectorData>();

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
            for (int i = 0; i < greyNodeIDs.Count; i++) {
                //get the current index
                int currentGreyNodeIndex = greyNodeIDs[i];
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
                Vector3 scaled = GraphConstructionManager.nodePositions[greyNodeIndex] * 7.0f;
                //create our VectorData from it
                VectorData pos = new VectorData(scaled);
                //use default rotation
                VectorData rot = new VectorData(Vector3.up);
                //update our level's obstacle position list
                GraphConstructionManager.GlobalLevelData.obstaclePositions.Add(pos);
                //update our level's obstacle rotation list
                GraphConstructionManager.GlobalLevelData.obstacleRotations.Add(rot);
                //keep track of the obstacle name since we found a place for it
                placedObstacleNames.Add(GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex]);
            }
        }

        IEnumerator DoubleNodeObstacleTypePlacement(int obstacleNameIndex)
        {
            int firstFoundGreyNodeIndex = -1;
            int secondFoundGreyNodeIndex = -1;

            for (int i = 0; i < greyNodeIDs.Count; i++) {

                int firstGreyNodeIndex = greyNodeIDs[i]; 
                Vector3 firstGreyNodePosition = GraphConstructionManager.nodePositions[firstGreyNodeIndex];

                for (int j = 0; j < greyNodeIDs.Count; j++) {

                    int secondGreyNodeIndex = greyNodeIDs[j]; 
                    Vector3 secondGreyNodePosition = GraphConstructionManager.nodePositions[secondGreyNodeIndex];

                    //dont' compare node to itself
                    if (i == j) {
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
                Vector3 firstScaled = GraphConstructionManager.nodePositions[firstFoundGreyNodeIndex] * 7.0f;
                Vector3 secondScaled = GraphConstructionManager.nodePositions[secondFoundGreyNodeIndex] * 7.0f;

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

                //instantiate obstacle
                string obstacleName = GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex];
                AssetReference assetRef = ObstacleDataRepository.data.Where(x => x.name == obstacleName).First().assetReference;
                AsyncOperationHandle<GameObject> loadHandle = assetRef.LoadAssetAsync<GameObject>();
                yield return loadHandle;
                GameObject go = Instantiate(loadHandle.Result, Vector3.zero, Quaternion.Euler(nodeDir), obstacleContainer.transform);

                //get the composite animation on the obstacle
                CompositeAnimation compositeAnimation = go.GetComponent<CompositeAnimation>();
                //Get all the composite animation's parts
                GameObject[] obstacleParts = compositeAnimation.animations.SelectMany<Obstacles.Animations.Animation, GameObject>(x => x.buildingBlocks).ToArray();
                //calculate an AABB on the part's positions
                Bounds partBounds = ML.Math.ComputeAABB(obstacleParts.Select(x => x.transform.position).ToList());
                //using this, we can determine the offset from the screen's center position 
                Vector3 CompositeAnimationOffset = nodesCentroid - partBounds.center;
                //then we can add this offset to the composite animation's position to center it properly
                Vector3 obstacleTargetPosition = compositeAnimation.gameObject.transform.position + CompositeAnimationOffset;

                //set obstacle position
                VectorData pos = new VectorData(obstacleTargetPosition);
                VectorData rot = new VectorData(nodeDir);
                GraphConstructionManager.GlobalLevelData.obstaclePositions.Add(pos);
                GraphConstructionManager.GlobalLevelData.obstacleRotations.Add(rot);
                placedObstacleNames.Add(GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex]);

                //clean up
                //Destroy(go);
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

            for (int i = 0; i < greyNodeIDs.Count; i++) {

                int firstGreyNodeIndex = greyNodeIDs[i]; 
                Vector3 firstGreyNodePosition = GraphConstructionManager.nodePositions[firstGreyNodeIndex];

                for (int j = 0; j < greyNodeIDs.Count; j++) {

                    int secondGreyNodeIndex = greyNodeIDs[j]; 
                    Vector3 secondGreyNodePosition = GraphConstructionManager.nodePositions[secondGreyNodeIndex];

                    //dont' compare node to itself
                    if (i == j) {
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
                Vector3 firstScaled = GraphConstructionManager.nodePositions[firstFoundGreyNodeIndex] * 7.0f;
                Vector3 secondScaled = GraphConstructionManager.nodePositions[secondFoundGreyNodeIndex] * 7.0f;

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
                placedObstacleNames.Add(GraphConstructionManager.GlobalLevelData.obstacleNames[obstacleNameIndex]);
            }
        }

        protected void OnNodeObstacleTypePlacement(int obstacleNameIndex)
        {

        }
    }

}