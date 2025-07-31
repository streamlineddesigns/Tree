using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.EventPublishers;
using StudioByStorm.ML.Clustering;

namespace StudioByStorm.Tutorials {

    public class DashTutorial : Tutorial
    {
        //we'll guide the player to swipe towards one of these node ids to demonstrate dashing
        [SerializeField] private int[] nodeIDsWithSwipeAnimation;
        private bool didPlayerDash = false;
        private bool wasPlayerStatic = false;

        protected void OnEnable()
        {
            GameEventPublisher.OnPlayerDash += OnPlayerDash;
        }

        protected void OnDisable()
        {
            base.OnDisable();
            GameEventPublisher.OnPlayerDash -= OnPlayerDash;
        }

        protected void OnPlayerDash()
        {
            GameManager.Singleton.PlayerController.SetRigidBodyType(RigidbodyType2D.Dynamic);
            didPlayerDash = true;
        }

        public override void Init()
        {
            int id = GameManager.Singleton.LevelManager.CurrentLevelData.safePath[1];
            nodeIDsWithSwipeAnimation = new int[1];
            nodeIDsWithSwipeAnimation[0] = id;
            GameManager.Singleton.PlayerController.isGravityAvailable = false;
        }

        protected override IEnumerator TutorialUpdate()
        {
            List<GameObject> allNodes = GameManager.Singleton.NodeRegistry.getAllAsList().Select(x => x.gameObject).ToList();

            while(isRunning) {
                if (!wasPlayerStatic) {
                    List<GameObject> nearbyNodes = KNN.GetKNearestNeighbors(GameManager.Singleton.player, allNodes, 1);
                    GameObject nearestNode = nearbyNodes[0];
                    if (nodeIDsWithSwipeAnimation.Contains(nearestNode.GetComponent<Node>().ID)) {
                        //prevent player from moving
                        wasPlayerStatic = true;
                        GameManager.Singleton.PlayerController.SetRigidBodyType(RigidbodyType2D.Static);
                        //show the finger swipe animation
                        GameManager.Singleton.FXManager.fingerSlingShotAnimation.Stop();
                        GameManager.Singleton.FXManager.fingerSlingShotAnimation.gameObject.SetActive(false);
                        GameObject startGO = GameManager.Singleton.player;
                        GameObject endGO = nearestNode;
                        GameManager.Singleton.FXManager.fingerSlingShotAnimation.SetPositions(startGO, endGO);
                        if (!GameManager.Singleton.FXManager.fingerSlingShotAnimation.isAnimating) {
                            GameManager.Singleton.FXManager.fingerSlingShotAnimation.gameObject.SetActive(true);
                            GameManager.Singleton.FXManager.fingerSlingShotAnimation.Animate();
                        }
                    }
                }
                
                yield return new WaitForSeconds(0.0333f);
            }
        }

        public override IEnumerator WaitUntilFinished()
        {
            yield return new WaitUntil(() => didPlayerDash);
            _isFinished = true;
        }

        public override void CleanUp()
        {
            Destroy(gameObject);
            GameManager.Singleton.PlayerController.SetRigidBodyType(RigidbodyType2D.Dynamic);
            GameManager.Singleton.FXManager.fingerSlingShotAnimation.Stop();
            GameManager.Singleton.FXManager.fingerSlingShotAnimation.gameObject.SetActive(false);
            GameManager.Singleton.PlayerController.isGravityAvailable = true;
        }
    }

}