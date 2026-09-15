using UnityEngine;
using System;
using StudioByStorm.Obstacles;

namespace StudioByStorm.Helpers {

    public class CircleCastHelper : MonoBehaviour
    {
        public GameObject originPoint;
        public float maxDistance = 5f;
        public float radius = 0.1f;
        public float minInput = 0.1f;
        public string[] tagsToCheck;
        public bool isDebugOn = false;
        public bool isScaling = false;
        public bool isFlippingDirection = false;
        public GameObject[] objectsToCast;
        public GameObject lineRenderPrefab;
        public LineRenderer[] lineRenderers;
        public float[] distances;
        public float[] tags;
        public NodeColor[] colorTypes;
        public GameObject[] hitGameObjects;
        public int layerMask;

        void Awake()
        {
            layerMask = LayerMask.GetMask("Default");
            distances = new float[objectsToCast.Length];
            tags = new float[objectsToCast.Length];
            lineRenderers = new LineRenderer[objectsToCast.Length];
            colorTypes = new NodeColor[objectsToCast.Length];
            hitGameObjects = new GameObject[objectsToCast.Length];
            GenerateLineRenderers();
        }


        public void SensorRayCast()
        {
            for (int i = 0; i < objectsToCast.Length; i++) {
                Vector3 tempDir = objectsToCast[i].transform.position - originPoint.transform.position;
                Vector3 direction = (isFlippingDirection) ? -tempDir : tempDir;
                direction.Normalize();
                float hitDistance = maxDistance;
                float tag = -1.0f;
                NodeColor colorType = NodeColor.GrayScale;
                GameObject hitGameObject;

                RaycastHit2D hit = (isFlippingDirection) ? Physics2D.CircleCast(objectsToCast[i].transform.position, radius, direction, maxDistance, layerMask) 
                                                         : Physics2D.CircleCast(originPoint.transform.position, radius, direction, maxDistance, layerMask);

                if (hit) {
                    hitGameObject = hit.collider.gameObject;
                    string colliderTag = hit.collider.gameObject.tag;
                    tag = Array.IndexOf(tagsToCheck, colliderTag);
                    hitDistance = hit.distance;

                    //cehck if we hit part of an obstacle
                    ObstaclePart obstaclePart = hit.collider.gameObject.GetComponent<ObstaclePart>();
                    colorType = (obstaclePart != null) ? obstaclePart.NodeColor : NodeColor.GrayScale;

                    if (isDebugOn) {
                        if (isFlippingDirection) {
                            lineRenderers[i].SetPosition(1, hit.point);
                            //Debug.DrawRay(objectsToCast[i].transform.position, direction * hit.distance, Color.yellow, 0.1f);
                        } else {
                            lineRenderers[i].SetPosition(1, hit.point);
                            //Debug.DrawRay(originPoint.transform.position, direction * hit.distance, Color.yellow, 0.1f);
                        }
                    }
                } else {
                    hitGameObject = new GameObject();
                    hitGameObject.name = "DESTROYME";
                    hitGameObject.transform.position =  (isFlippingDirection) ? objectsToCast[i].transform.position : originPoint.transform.position;

                    if (isFlippingDirection) {
                        lineRenderers[i].SetPosition(1, objectsToCast[i].transform.position);
                    } else {
                        lineRenderers[i].SetPosition(1, originPoint.transform.position);
                    }
                }
                            
                tags[i] = (isScaling) ? tag + minInput : tag;
                distances[i] = (isScaling) ? hitDistance + minInput : hitDistance;
                colorTypes[i] = colorType;
                hitGameObjects[i] = hitGameObject;
            }

            if (isScaling) {
                distances = ML.Math.MinMaxScale(distances, 0, maxDistance + minInput);
                tags = ML.Math.MinMaxScale(tags, -1.0f, (tagsToCheck.Length - 1) + minInput);
            }

            for (int j = 0; j < hitGameObjects.Length; j++) {
                if (hitGameObjects[j].name == "DESTROYME") Destroy(hitGameObjects[j]);
            }
            
        }

        protected void GenerateLineRenderers()
        {
            for (int i = 0; i < objectsToCast.Length; i++) {
                GameObject go;

                if (isFlippingDirection) {
                    go = Instantiate(lineRenderPrefab, objectsToCast[i].transform.position, Quaternion.identity, originPoint.transform);
                    lineRenderers[i] = go.GetComponent<LineRenderer>(); 
                    lineRenderers[i].SetPosition(0, objectsToCast[i].transform.position);
                } else {
                    go = Instantiate(lineRenderPrefab, originPoint.transform.position, Quaternion.identity, originPoint.transform);
                    lineRenderers[i] = go.GetComponent<LineRenderer>();
                    lineRenderers[i].SetPosition(0, originPoint.transform.position);
                }
                
                lineRenderers[i].widthMultiplier = 3.0f;
                go.SetActive(true);
            }
        }

    }

}