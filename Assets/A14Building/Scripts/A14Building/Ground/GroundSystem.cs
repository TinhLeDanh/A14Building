using Core.Utilities;
using A14Building.Build;
using UnityEngine;

namespace A14Building.Ground
{
    public class GroundSystem : Singleton<GroundSystem>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        //Width, Height of 1 tile
        public Vector2 UnitSize = Vector2.one;

        //Map size
        public Vector2 GridSize;

        
    }
}

