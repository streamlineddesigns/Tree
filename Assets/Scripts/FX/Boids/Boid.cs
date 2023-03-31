using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using StudioByStorm.Optimizations;

namespace StudioByStorm.FX.Boids {

    public class Boid : MonoBehaviour
    {
        public Vector2 cellID;
        public NodeColor Color;
        public SpriteRenderer SpriteRenderer;
        public int targetID;
        protected Rect bounds;
        protected List<Boid> nearbyBoids;
        protected Vector2 acceleration;
        protected Vector2 velocity;
        protected Vector2 alignmentVector;
        protected Vector2 separationVector;
        protected Vector2 cohesionVector;
        protected Vector2 targetVector;
        protected HashData HashData;

        public void SetColor(Color color)
        {
            SpriteRenderer.color = color;
        }

        protected void Start()
        {
            float angle = Random.Range(0, 2 * Mathf.PI);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + GameManager.Singleton.FXManager.BoidConfig.baseRotation);
            velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            targetID = 0;
            
            HashData = new HashData(gameObject, this);
            GameManager.Singleton.FXManager.SpatialHash.AddObject(HashData);
        }

        protected void FixedUpdate()
        {   
            NearbyQuery();
            Flock();
            UpdateVelocity();
            UpdatePosition();
            UpdateRotation();
            WrapAround();
        }

        protected void NearbyQuery()
        {
            cellID = GameManager.Singleton.FXManager.SpatialHash.GetCellIDForObj(gameObject);
            GameManager.Singleton.FXManager.SpatialHash.UpdateObject(HashData);
            nearbyBoids = GameManager.Singleton.FXManager.SpatialHash.GetNearby(cellID).Select(x => x.GetData<Boid>()).ToList();
            nearbyBoids.Remove(this);
        }

        protected void Flock()
        {
            UpdateVectors();

            Vector2 target = Target();
            Vector2 alignment = Alignment();
            Vector2 separation = Separation();
            Vector2 cohesion = Cohesion();

            acceleration +=   GameManager.Singleton.FXManager.BoidConfig.targetAmount * target 
                            + GameManager.Singleton.FXManager.BoidConfig.alignmentAmount * alignment 
                            + GameManager.Singleton.FXManager.BoidConfig.cohesionAmount * cohesion 
                            + GameManager.Singleton.FXManager.BoidConfig.separationAmount * separation;
        }

        protected void UpdateVectors()
        {
            acceleration = Vector2.zero;
            targetVector = Vector2.zero;
            alignmentVector = Vector2.zero;
            separationVector = Vector2.zero;
            cohesionVector = Vector2.zero;

            for (int i = 0; i < nearbyBoids.Count; i++) {
                alignmentVector += nearbyBoids[i].velocity;

                Vector2 difference = transform.position - nearbyBoids[i].gameObject.transform.position;
                separationVector += difference.normalized / difference.magnitude;

                cohesionVector += (Vector2) nearbyBoids[i].gameObject.transform.position;
            }
        }

        protected Vector2 Target()
        {
            if (GameManager.Singleton.FXManager.BoidTargets != null && GameManager.Singleton.FXManager.BoidTargets.Length > 0) {
                Vector3 offsetToTarget = (GameManager.Singleton.FXManager.BoidTargets[targetID].position - transform.position);
                if (offsetToTarget.x < 0.1f && offsetToTarget.y < 0.1f) {
                    targetID = (targetID >= GameManager.Singleton.FXManager.BoidTargets.Length - 1) ? 0 : targetID + 1; //Random.Range(0, GameManager.Singleton.FXManager.BoidTargets.Length);
                } else {
                    targetVector = Steer(offsetToTarget.normalized * GameManager.Singleton.FXManager.BoidConfig.maxSpeed / 2);
                }
            }

            return targetVector;
        }

        protected void UpdateVelocity()
        {
            velocity += acceleration;
            velocity = LimitMagnitude(velocity, GameManager.Singleton.FXManager.BoidConfig.maxSpeed);
        }

        protected void UpdatePosition()
        {
            transform.position += (Vector3) velocity * Time.deltaTime;
        }

        protected void UpdateRotation()
        {
            var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle) + GameManager.Singleton.FXManager.BoidConfig.baseRotation);
        }

        protected Vector2 Alignment()
        {
            if (!nearbyBoids.Any()) return alignmentVector;

            alignmentVector /= nearbyBoids.Count();
            var steer = Steer(alignmentVector.normalized * GameManager.Singleton.FXManager.BoidConfig.maxSpeed);
            return steer;
        }

        protected Vector2 Cohesion()
        {
            if (!nearbyBoids.Any()) return Vector2.zero;

            var average = cohesionVector / nearbyBoids.Count();
            var direction = average - (Vector2) transform.position;
            var steer = Steer(direction.normalized * GameManager.Singleton.FXManager.BoidConfig.maxSpeed);
            return steer;
        }

        protected Vector2 Separation()
        {
            if (!nearbyBoids.Any()) return separationVector;

            separationVector /= nearbyBoids.Count();

            var steer = Steer(separationVector.normalized * GameManager.Singleton.FXManager.BoidConfig.maxSpeed);
            return steer;
        }

        protected Vector2 Steer(Vector2 desired)
        {
            var steer = desired - velocity;
            steer = LimitMagnitude(steer, GameManager.Singleton.FXManager.BoidConfig.maxForce);

            return steer;
        }

        protected Vector2 LimitMagnitude(Vector2 baseVector, float maxMagnitude)
        {
            if (baseVector.sqrMagnitude > maxMagnitude * maxMagnitude)
            {
                baseVector = baseVector.normalized * maxMagnitude;
            }
            return baseVector;
        }

        /*
         * Can wrap an object around any direction of the screen, but we could also just deactive the boid since it will be pooled
         */
        protected void WrapAround()
        {
            if (transform.position.x < GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.x - 40f) transform.position = new Vector2(GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.x + 40f, GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.y);
            if (transform.position.y < GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.y - 30f) transform.position = new Vector2(GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.x, GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.y + 30f);
            if (transform.position.x > GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.x + 40f) transform.position = new Vector2(GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.x - 40f, GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.y);
            if (transform.position.y > GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.y + 30f) transform.position = new Vector2(GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.x, GameManager.Singleton.LevelManager.CurrentLevelData.Centroid.y - 30f);
        }

    }

}