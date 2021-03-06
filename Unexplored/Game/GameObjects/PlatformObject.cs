﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unexplored.Core.Base;
using Unexplored.Core.Components;
using Unexplored.Core.Physics;
using Unexplored.Core.Types;
using Unexplored.Game.Components;

namespace Unexplored.Game.GameObjects
{
    public class PlatformObject : GameObject
    {
        public PlatformObject() : base()
        {
            SetComponents(
                new PlatformControllerComponent(),
                new RigidbodyComponent(false, true),
                new ColliderComponent(true, MapCollider.Create(new Vector2(0, 0), new Vector2(0, 0), "collider")) { ForceNotify = true },
                new SpriteRendererComponent(45)
                );
        }
    }
}
