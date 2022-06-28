using A14Building.Build;
using Core.Utilities;
using UnityEngine;

namespace A14Building.System
{
    public class GameSystem : Singleton<GameSystem>
    {
        [HideInInspector]
        public Plane xzPlane;

        public GameObject BuildingBase;
        public Building buildingSelected = null;

        protected override void Awake()
        {
            base.Awake();
        }

        public void BuildingLandUnselect()
        {
            if (buildingSelected == null) return;

            //buildingSelected.Land(true, true);
            buildingSelected = null;
            //Save();

            //UICommand.Hide();
        }
    }
}

