using A14Building.System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace A14Building.Build
{
    public class Building : MonoBehaviour
    {
        // remember old tile pos while drag building
        private Vector2 tilePosOld = new Vector2(0, 0);
        // tile position
        public Vector2 tilePos = new Vector2(0, 0);
        // building's size
        public Vector2 tileSize = new Vector2(1, 1);

        // current building's land state
        [HideInInspector]
        public bool Landed = false;

        [HideInInspector]
        public GroundSystem ground = null;

        public void Init(int type, int level)
        {

        }
    }
}

