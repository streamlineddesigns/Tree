using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StudioByStorm.Data;
using StudioByStorm.Config;
using UnityEngine.SceneManagement;

namespace StudioByStorm.Graph {

    public class GraphConstructionManager : MonoBehaviour
    {
        public LevelConfig LevelConfig;
        public LevelData GlobalLevelData;
        public ColorModel ColorModel;
        public GameObject NodePrefab;
        public Transform NodeParent;
        public Transform EdgeParent;
        public Material lineRendererMaterial;
        private GameObject tappedGameObject;
        private GameObject pressedGameObject;
        private GameObject panBeginGameObject;
        private GameObject panCompleteGameObject;
        private bool isPanning;
        private Vector2 panTouchLocation;
        private TKTapRecognizer tapRecognizer;
        private TKLongPressRecognizer longPressRecognizer;
        private TKPanRecognizer panRecognizer;
        public Dictionary<Vector3, int> rowPopulationCount = new Dictionary<Vector3, int>();
        public NodeColor currentCursorNodeColor = NodeColor.GrayScale;
        public List<NodeColor> nodeColors = new List<NodeColor>();
        public List<Vector3> nodePositions = new List<Vector3>();
        private Dictionary<Vector3, GameObject> nodeGameObjects = new Dictionary<Vector3, GameObject>();
        private List<LineRenderer> edgeLineRenderers = new List<LineRenderer>();
        public AdjacencyList AdjacencyList = new AdjacencyList();

        protected void Awake()
        {
            tapRecognizer = new TKTapRecognizer();
            longPressRecognizer = new TKLongPressRecognizer();
            panRecognizer = new TKPanRecognizer();
        }

        public void BlueButtonClick()
        {
            currentCursorNodeColor = NodeColor.Blue;
        }

        public void GreenButtonClick()
        {
            currentCursorNodeColor = NodeColor.Green;
        }   

        public void PurpleButtonClick()
        {
            currentCursorNodeColor = NodeColor.Purple;
        }

        public void WhiteButtonClick()
        {
            currentCursorNodeColor = NodeColor.White;
        }

        public void BackButtonClick()
        {
            SceneManager.LoadScene("Main");
        }

        public void GrayScaleButtonClick()
        {
            currentCursorNodeColor = NodeColor.GrayScale;
        }

        protected void Start()
        {
            
        }

        protected void OnEnable()
        {
            tapRecognizer.gestureRecognizedEvent += OnTap;
            TouchKit.addGestureRecognizer(tapRecognizer);

            longPressRecognizer.gestureRecognizedEvent += OnPanBegin;
            longPressRecognizer.gestureCompleteEvent += OnPanComplete;
            longPressRecognizer.allowableMovementCm = 25;
            TouchKit.addGestureRecognizer(longPressRecognizer);
        }

        protected void OnDisable()
        {
            tapRecognizer.gestureRecognizedEvent -= OnTap;
            longPressRecognizer.gestureRecognizedEvent -= OnPanBegin;
            longPressRecognizer.gestureCompleteEvent -= OnPanComplete;
        }

        protected void OnPanBegin(TKLongPressRecognizer r)
        {
            setPanGameObject(r.startTouchLocation(), true);

            if (validateNodeGameObject(panBeginGameObject)) {
                createEdgeStart(panBeginGameObject.transform.position);
            }   
        }

        protected void OnPanComplete(TKLongPressRecognizer r)
        {
            setPanGameObject(r.touchLocation(), false);

            if (validateNodeGameObject(panCompleteGameObject) && validateNodeGameObject(panBeginGameObject)) {
                createEdgeEnd(panCompleteGameObject.transform.position);
            } else {

                if (validateNodeGameObject(panBeginGameObject)) {
                    deletePreviousEdgeEnd();
                }
                
            }   
        }

        protected void OnLongPressBegin(TKLongPressRecognizer r)
        {
            setPressedGameObject(r.startTouchLocation());
        }

        protected void OnTap(TKTapRecognizer r) 
        {
            setTappedGameObject(r.startTouchLocation());
            editTappedGameObject();
        }

        protected void Update()
        {
            if (isPanning) {
                bool hasANode = false;
                GameObject go = getGameObject(longPressRecognizer.touchLocation());
                if (go != null) {
                    hasANode = hasNode(go.transform.position);
                }
                Vector3 point = (hasANode) ? go.transform.position : Camera.main.ScreenToWorldPoint(longPressRecognizer.touchLocation());
                edgeLineRenderers[edgeLineRenderers.Count - 1].SetPosition(1, point);
            }
        }

        protected void setTappedGameObject(Vector3 position) 
        {
            tappedGameObject = getGameObject(position);
        }

        protected void setPressedGameObject(Vector3 position) 
        {
            pressedGameObject = getGameObject(position);
        }

        protected void setPanGameObject(Vector3 position, bool panBegin) 
        {
            GameObject go = getGameObject(position);

            if (panBegin) {
                panBeginGameObject = go;
            } else {
                panCompleteGameObject = go;
            }
        }

        protected GameObject getGameObject(Vector3 position)
        {
            Ray ray = Camera.main.ScreenPointToRay(position);
            RaycastHit2D hit = Physics2D.Raycast (ray.origin, ray.direction, Mathf.Infinity);
            return (hit.collider != null) ? hit.collider.gameObject : null;
        }

        protected void editTappedGameObject() {
            if (tappedGameObject == null) {
                return;
            }

            switch(tappedGameObject.tag) {
                case "Cell":
                    editCell();
                    break;
                case "Interactable":
                    moveRow();
                    break;
            }
        }

        protected void editCell()
        {
            Vector3 cellPosition = tappedGameObject.transform.position;

            if (hasNode(cellPosition)) {
                removeNode(cellPosition);
            } else {
                addNode(cellPosition);
            }
        }

        protected bool validateNodeGameObject(GameObject go)
        {
            if (go != null && hasNode(go.transform.position)) {
                return true;
            }
            return false;
        }

        protected bool hasNode(Vector3 cellPosition)
        {
            if (nodePositions.Contains(cellPosition)) {
                return true;
            }
            return false;
        }

        protected void addNode(Vector3 cellPosition)
        {
            nodePositions.Add(cellPosition);
            AdjacencyList.Add(nodePositions.Count - 1);
            nodeColors.Add(currentCursorNodeColor);
            GrayScaleButtonClick();
            GameObject nodeGameObject = Instantiate(NodePrefab, cellPosition, Quaternion.identity, NodeParent);
            nodeGameObject.GetComponent<SpriteRenderer>().color = ColorModel.lightColor[(int)nodeColors[nodePositions.Count - 1]];
            nodeGameObjects.Add(cellPosition, nodeGameObject);
            IncrementRowPopulationCount();
        }

        protected void removeNode(Vector3 cellPosition)
        {
            int index = nodePositions.IndexOf(cellPosition);
            
            if (! hasAnyEdgeInAdjacencyList(index)) {
                nodePositions.Remove(cellPosition);
                AdjacencyList.Remove(index);
                nodeColors.RemoveAt(index);
                GameObject nodeReference = nodeGameObjects[cellPosition];
                nodeGameObjects.Remove(cellPosition);
                Destroy(nodeReference);
                DecrementRowPopulationCount();
            }
        }
        
        protected void createEdgeStart(Vector3 position)
        {
            if (hasNode(position)) {
                GameObject go = new GameObject("LineRenderer");
                go.transform.SetParent(EdgeParent);
                LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
                Color lightBlack = new Color(49, 49, 49, 1);
                lineRenderer.SetColors (lightBlack, Color.black);
                lineRenderer.material = lineRendererMaterial;
                lineRenderer.SetWidth(0.1f, 0.1f);
                lineRenderer.sortingOrder = 2;
                lineRenderer.SetPosition(0, panBeginGameObject.transform.position);
                edgeLineRenderers.Add(lineRenderer);
                isPanning = true;   
            }
        }

        protected void deletePreviousEdgeEnd()
        {
            createEdgeEnd(Vector3.zero, true);
        }

        protected void createEdgeEnd(Vector3 position, bool delete = false)
        {
            int index1 = -1;
            int index2 = -1;
            if (panBeginGameObject != null && panCompleteGameObject != null) {
                index1 = nodePositions.IndexOf(panBeginGameObject.transform.position);//start node
                index2 = nodePositions.IndexOf(panCompleteGameObject.transform.position);//end node;
            }

            if (hasNode(position) && ! delete && !hasEdgeInAdjacencyList(index1, index2) && index1 != index2) {

                edgeLineRenderers[edgeLineRenderers.Count - 1].SetPosition(1, position);
                addToAdjacencyList(index1, index2);
                
            } else {
                if (! hasNode(position) && ! delete) {
                    return;
                }
                int index = edgeLineRenderers.Count - 1;
                LineRenderer lineRendererReference = edgeLineRenderers[index];
                edgeLineRenderers.RemoveAt(index);
                Destroy(lineRendererReference.gameObject);
            }
            isPanning = false;
        }

        protected bool hasAnyEdgeInAdjacencyList(int index)
        {
            if (AdjacencyList.Count(index) > 0) {
                return true;
            }

            return false;
        }

        protected bool hasEdgeInAdjacencyList(int index1, int index2)
        {
            if (AdjacencyList.Contains(index1, index2)) {
                return true;
            }

            return false;
        }

        protected void addToAdjacencyList(int index1, int index2)
        {
            if (! AdjacencyList.Contains(index1, index2)) {
                AdjacencyList.Add(index1, index2);
            }
            if (! AdjacencyList.Contains(index2, index1)) {
                AdjacencyList.Add(index2, index1);
            }
        }

        public void showAdjacencyList()
        {
            AdjacencyList singleEntryAdjacencyList = AdjacencyList.GetWithoutDuplicateEdges();
            singleEntryAdjacencyList.Log();

            LevelData LevelData = new LevelData();
            LevelData.Layers = new List<LayerData>();

            LevelData.AdjacencyListData = singleEntryAdjacencyList.GetAll();

            Dictionary<float, int> Layers = new Dictionary<float, int>();
            for (int i = 0; i < nodePositions.Count; i++) {
                LayerData CurrentLayerData = new LayerData();
                if (! Layers.ContainsKey(nodePositions[i].y)) {
                    Layers.Add(nodePositions[i].y, 1);
                } else { 
                    Layers[nodePositions[i].y]++;
                }
            }

            int index = 0;
            List<float> Keys = Layers.Select(x => x.Key).ToList();
            for (int j = 0; j < Keys.Count; j++) {
                int layerNodeCount = Layers[Keys[j]];
                LayerData LayerData = new LayerData();
                LayerData.nodeCount = layerNodeCount;
                LayerData.nodePositions = new List<VectorData>();
                LayerData.nodeColors = new List<NodeColor>();
                LayerData.nodeTypes = new List<NodeType>();
                for (int k = 0; k < layerNodeCount; k++) {
                    Vector3 scaled = nodePositions[index] * 7.0f;
                    VectorData pos = new VectorData(scaled);
                    LayerData.nodePositions.Add(pos);
                    LayerData.nodeColors.Add(nodeColors[index]);
                    NodeType nodeType = (nodeColors[index] != NodeColor.GrayScale) ? NodeType.Parent : NodeType.Disjoint;
                    LayerData.nodeTypes.Add(nodeType);
                    index++;
                }
                LevelData.Layers.Add(LayerData);
            }

            GlobalLevelData = LevelData;
            //Debug.Log(LevelData.Layers.Count);

            string dir = Application.persistentDataPath;
            
            int LevelFileCountInDir = Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length;
            string LevelSaveFilePath = (dir + LevelConfig.fileNameAppend + LevelFileCountInDir + LevelConfig.fileNamePrepend).ToString();

            Debug.Log(LevelSaveFilePath);

            File.WriteAllText(LevelSaveFilePath, JsonConvert.SerializeObject(LevelData, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        }

        protected void moveRow()
        {
            Transform row = tappedGameObject.transform.parent;
            if (rowPopulationCount.ContainsKey(row.position) && rowPopulationCount[row.position] > 0) {
                return;
            }
            Vector3 targetPosition = row.position;
            targetPosition.x = (row.position.x == 0) ? 0.5f: 0.0f;
            row.position = targetPosition;
        }

        protected void IncrementRowPopulationCount()
        {
            Vector3 rowPosition = tappedGameObject.transform.parent.position;

            if (rowPopulationCount.ContainsKey(rowPosition)) {
                int count = rowPopulationCount[rowPosition] + 1;
                rowPopulationCount[rowPosition] = count;
            } else {
                rowPopulationCount.Add(rowPosition, 1);
            }
        }

        protected void DecrementRowPopulationCount()
        {
            Vector3 rowPosition = tappedGameObject.transform.parent.position;

            int count = rowPopulationCount[rowPosition] - 1;
            rowPopulationCount[rowPosition] = count;
        }
    }

}