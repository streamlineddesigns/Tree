using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class ActionView : MonoBehaviour
    {
        public ActionModel ActionModel;
        public Image TravelButtonImage;
        public Image DragTravelButtonImage;
        public Image GetButtonImage;
        public Image SetButtonImage;
        public Button GetEdgeButton;
        public Button SetEdgeButton;
        public Button TravelEdgeButton;
        private Dictionary<NodeColor, int> connectedParentsCount = new Dictionary<NodeColor, int>();

        public void EnableTravelButton()
        {
            TravelEdgeButton.interactable = true;
            SetActivationColors(TravelEdgeButton, GameManager.Singleton.ColorModel.ColoredTravelers, TravelButtonImage);
        }

        public void DisableTravelButton()
        {
            TravelEdgeButton.interactable = false;
            DeactivationColor(TravelButtonImage, GameManager.Singleton.ColorModel.ColoredTravelers);
        }

        public void EnableGetEdgeButton() {
            GetEdgeButton.interactable = true;
            SetActivationColors(GetEdgeButton, GameManager.Singleton.ColorModel.ColoredGetters, GetButtonImage);
        }

        public void DisableGetEdgeButton() {
            GetEdgeButton.interactable = false;
            DeactivationColor(GetButtonImage, GameManager.Singleton.ColorModel.ColoredGetters);
        }

        public void EnableSetEdgeButton() {
            SetEdgeButton.interactable = true;
            SetActivationColors(SetEdgeButton, GameManager.Singleton.ColorModel.ColoredSetters, SetButtonImage);
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
            GameManager.Singleton.player.GetComponent<ActionController>().ActionModel.CurrentEdge = edge; //$$cyclic dependencies. need a central location for this data seperated from player action and action view
        }

        public void SetEdgeButtonClick(Edge edge)
        {
            //need to get the nearest nodes id
            Node nearestNode = GameManager.Singleton.nearbyNode.GetData<Node>();
            //set new target
            edge.FabrikSolver2D.GetChain(edge.FabrikSolver2D.chainCount).target = nearestNode.gameObject.transform;
            //remove edge from player
            GameManager.Singleton.player.GetComponent<ActionController>().ActionModel.CurrentEdge = null;
        }

        public void TravelButtonClick()
        {
            

        }
    }

}