﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unexplored.Core.Components;
using Unexplored.Core.Physics;
using Unexplored.Core.Types;

namespace Unexplored.Core
{
    public static class Physics2D
    {
        private static List<Rigidbody> rigidbodies;
        private static List<ICollider> colliders;

        private static QuadNode<ICollider> quadTree;
        private static List<ICollider> list;

        private static List<Manifold> contacts;
        private static Vector2 gravity;
        private static int iterations;
        private const float EPSILON = 0.0001f;

        public static void AddCollider(ICollider collider)
        {
            if (!colliders.Contains(collider))
                colliders.Add(collider);
        }

        public static void AddRigidbody(Rigidbody rigidbody)
        {
            if (!rigidbodies.Contains(rigidbody))
                rigidbodies.Add(rigidbody);
        }

        public static void RemoveCollider(ICollider collider)
        {
            colliders.Remove(collider);
        }

        public static void RemoveRigidbody(Rigidbody rigidbody)
        {
            rigidbodies.Remove(rigidbody);
        }

        static Physics2D()
        {
            colliders = new List<ICollider>();
            rigidbodies = new List<Rigidbody>();
        }

        public static void Initialize()
        {
            quadTree = new QuadNode<ICollider>(null, new FRect(0, 0, 2048, 2048), 0);
            list = new List<ICollider>();

            contacts = new List<Manifold>(128);
            iterations = 1;
            gravity = new Vector2(0, 1000f);
        }

        public static void Update()
        {
            quadTree.Clear();
            var index = colliders.Count;
            while (--index >= 0)
            {
                quadTree.Insert(colliders[index]);
            }
        }

        public static void FindCollisions()
        {
            contacts.Clear();
            
            for (int i = 0; i < colliders.Count; i++)
            {
                var a = colliders[i];
                for (int j = i + 1; j < colliders.Count; j++)
                {
                    var b = colliders[j];

                    //if (a.AABB.Id == b.AABB.Id)
                        //continue;

                    if (!a.Rigidbody.IsKinematic && !b.Rigidbody.IsKinematic)
                        continue;

                    if (a.AABB.IsOverlapping(b.AABB))
                    {
                        if (!a.ResolveToCollide || !b.ResolveToCollide)
                        {
                            a.OwnGameObject.OnTriggerEnter(new Trigger(b.OwnGameObject, b.AABB));
                            b.OwnGameObject.OnTriggerEnter(new Trigger(a.OwnGameObject, a.AABB));
                        }
                        else // if (a.IsTrigger)
                        {
                            if (a.OwnGameObject is Game.GameObjects.HeroObject &&
                                b.OwnGameObject is Game.GameObjects.EnemyObject ||
                                a.OwnGameObject is Game.GameObjects.EnemyObject &&
                                b.OwnGameObject is Game.GameObjects.HeroObject)
                            {

                            }

                            var c = new Manifold(a.Rigidbody, b.Rigidbody);
                            if (c.Solve())
                            {
                                contacts.Add(c);

                                if (a.IsCollision)
                                {
                                    a.OwnGameObject.OnCollision(new Collision { Manifold = c, GameObject = b.OwnGameObject, OtherRigidbody = b.Rigidbody });
                                }

                                if (b.IsCollision)
                                {
                                    b.OwnGameObject.OnCollision(new Collision { Manifold = c, GameObject = a.OwnGameObject, OtherRigidbody = a.Rigidbody });
                                }
                            }
                            else
                                c = null;
                        }
                    }
                }
            }
        }

        public static void IntegrateForces(float dt)
        {
            int rigidbodiesCount = rigidbodies.Count;
            for (int i = 0; i < rigidbodiesCount; i++)
            {
                rigidbodies[i].IntegrateForces(gravity, dt);
            }
        }

        public static void SolveCollisions()
        {
            int l = contacts.Count;
            for (int i = 0; i < iterations; i++)
            {
                for (int c = 0; c < l; c++)
                {
                    contacts[c].Resolve(EPSILON);
                }
            }
        }

        public static void IntegrateVelocities(float dt)
        {
            int rigidbodiesCount = rigidbodies.Count;
            for (int i = 0; i < rigidbodiesCount; i++)
            {
                rigidbodies[i].IntegrateVelocity(gravity, dt);
                rigidbodies[i].ClearForces();
                rigidbodies[i].Box.UpdateBounds();
            }
        }

        public static void CorrectPositions()
        {
            int l = contacts.Count;
            for (int i = 0; i < iterations; i++)
            {
                for (int c = 0; c < l; c++)
                {
                    contacts[c].PositionalCorrection();
                }
            }
        }

        public static void Tick(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            FindCollisions();
            IntegrateForces(dt);
            SolveCollisions();
            IntegrateVelocities(dt);
            CorrectPositions();
        }
        
        internal static void Clear()
        {
            colliders.Clear();
            rigidbodies.Clear();
        }
    }
}
