using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lean.Gui;

namespace StudioByStorm.UI {

    public class ActionView : View
    {
        public float speed = 5.0f;
        public float pMovementThreshold = 0.1f;
        public LeanJoystick JumpJoyStick;
        public LeanJoystick TravelLeanJoyStick;
        public LeanJoystick LeanJoyStick;
        public RectTransform JumpJoyStickRect;
        public RectTransform JumpJoyStickHandleRect;
        public GameObject pAnchor;
        public GameObject p2HandleTarget;
        public GameObject p3HandleTarget;
        public GameObject p2GO;
        public GameObject p3GO;
        public GameObject p2Indicator;
        public GameObject p3Indicator;
        public ActionModel ActionModel;
        public Image TravelButtonImage;
        public Image DragTravelButtonImage;
        public Image GetButtonImage;
        public Image SetButtonImage;
        public Button GetEdgeButton;
        public Button SetEdgeButton;
        public Button TravelEdgeButton;
        protected Vector3 previousp0Position;
        protected Dictionary<NodeColor, int> connectedParentsCount = new Dictionary<NodeColor, int>();

        public void EnableGetEdgeButton() {
            GetEdgeButton.interactable = true;
            SetActivationColors(GetEdgeButton, GameManager.Singleton.ColorModel.ColoredGetters, GetButtonImage);
            GetEdgeButton.gameObject.SetActive(true);
            SetEdgeButton.gameObject.SetActive(false);
        }

        public void DisableGetEdgeButton() {
            GetEdgeButton.interactable = false;
            DeactivationColor(GetButtonImage, GameManager.Singleton.ColorModel.ColoredGetters);
        }

        public void EnableSetEdgeButton() {
            SetEdgeButton.interactable = true;
            SetActivationColors(SetEdgeButton, GameManager.Singleton.ColorModel.ColoredSetters, SetButtonImage);
            SetEdgeButton.gameObject.SetActive(true);
            GetEdgeButton.gameObject.SetActive(false);
        }

        public void DisableSetEdgeButton() {
            SetEdgeButton.interactable = false;
            DeactivationColor(SetButtonImage, GameManager.Singleton.ColorModel.ColoredSetters);
            
        }

        protected void SetActivationColors(Button Button, Sprite[] coloredSprites, Image image)
        {
            int colorIndex = -1;

            if (ActionModel.CurrentEdge != null) {
                colorIndex = (int) ActionModel.CurrentEdge.EdgeColor;
            } else {
                colorIndex = (int) ActionModel.CurrentNode.NodeColor;
            }

            if (colorIndex != -1) {
                image.sprite = coloredSprites[colorIndex];
                ColorBlock cb = Button.colors;
                cb.normalColor = GameManager.Singleton.ColorModel.lightColor[colorIndex];
                cb.highlightedColor = GameManager.Singleton.ColorModel.lightColor[colorIndex];
                cb.pressedColor = GameManager.Singleton.ColorModel.darkColor[colorIndex];
                cb.selectedColor = GameManager.Singleton.ColorModel.lightColor[colorIndex];
                Button.colors = cb;
            }
        }

        protected void DeactivationColor(Image image, Sprite[] coloredSprites)
        {
            //if (ActionModel.CurrentEdge == null) {
                int colorIndex = (int) NodeColor.GrayScale;
                image.sprite = coloredSprites[colorIndex];
            //}
        }

        public void GetEdgeButtonClick()
        {
            //need to get the nearest nodes id
            int nearestNodeID = GameManager.Singleton.nearbyNode.GetData<Node>().ID;
            //then check the edge registry for an edge with the same id
            Edge edge = GameManager.Singleton.EdgeRegistry.TryGetValue(nearestNodeID);
            //then activate it
            edge.gameObject.SetActive(true);
            //add edge to player
            ActionController ActionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName) as ActionController;
            ActionController.ActionModel.CurrentEdge = edge; //$$cyclic dependencies. need a central location for this data seperated from player action and action view
        }

        public void SetEdgeButtonClick(Edge edge)
        {
            //remove edge from player
            ActionController ActionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName) as ActionController;
            ActionController.ActionModel.CurrentEdge = null;
        }

        public void TravelButtonClick()
        {
            

        }

        public void OnJumpJoyStickDown()
        {/*
            //get positions
            Vector3 p0 = JumpJoyStickHandleRect.gameObject.transform.localPosition;
            if (ML.Math.GetDistance(p0, previousp0Position) > pMovementThreshold) {
                previousp0Position = p0;
            } else {
                return;
            }
            
            Vector3 p1 = pAnchor.transform.localPosition;
            Vector3 dir = (p1 - p0).normalized;

            float angle2 = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion q2 = Quaternion.AngleAxis(angle2, Vector3.forward);
            pAnchor.transform.rotation = Quaternion.Slerp(pAnchor.transform.rotation, q2, Time.deltaTime * speed);

            Vector3 p2TargetPosition = dir - (pAnchor.transform.up * 100.0f);
            Vector3 p3TargetPosition = dir + (pAnchor.transform.up * 100.0f);

            p2GO.transform.localPosition = p2TargetPosition;
            p3GO.transform.localPosition = p3TargetPosition;


            //set indicator rotations
            Vector3 p2dir = (p2TargetPosition - p0).normalized;
            float angleIndicator2 = Mathf.Atan2(p2dir.y, p2dir.x) * Mathf.Rad2Deg;
            Quaternion qIndicator2 = Quaternion.AngleAxis(angleIndicator2, Vector3.forward);
            p2Indicator.transform.rotation = Quaternion.Slerp(pAnchor.transform.rotation, qIndicator2, Time.deltaTime * speed);
            
            Vector3 p3dir = (p3TargetPosition - p0).normalized;
            float angleIndicator3 = Mathf.Atan2(p3dir.y, p3dir.x) * Mathf.Rad2Deg;
            Quaternion qIndicator3 = Quaternion.AngleAxis(angleIndicator3, Vector3.forward);
            p3Indicator.transform.rotation = Quaternion.Slerp(pAnchor.transform.rotation, qIndicator3, Time.deltaTime * speed);



            //set inidicator positions
            p2Indicator.transform.localPosition = p2TargetPosition - p2Indicator.transform.right * 75f;
            p3Indicator.transform.localPosition = p3TargetPosition - p3Indicator.transform.right * 75f;

            //set indicator size
            float p2Distance = ML.Math.GetDistance((Vector2) p0, (Vector2) p2TargetPosition);
            float p3Distance = ML.Math.GetDistance((Vector2) p0, (Vector2) p3TargetPosition);
            p2Indicator.transform.localScale = new Vector3(p2Distance, p2Indicator.transform.localScale.y, p2Indicator.transform.localScale.z);
            p3Indicator.transform.localScale = new Vector3(p3Distance, p3Indicator.transform.localScale.y, p3Indicator.transform.localScale.z);
            */
        }

        public void OnJumpJoyStickUp()
        {
            
        }
    }

}