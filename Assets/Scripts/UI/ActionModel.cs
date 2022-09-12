using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioByStorm.UI {

    public class ActionModel : MonoBehaviour
    {
        public Node CurrentNode;
        public Edge CurrentEdge;
        public int[] colorMaxConnections;
        public int[] ColorConnectionsCount;
    }

}