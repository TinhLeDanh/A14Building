using Core.Utilities;
using A14Building.Build;
using UnityEngine;
using System.Collections.Generic;

namespace A14Building.System
{
    public class GroundSystem : Singleton<GroundSystem>
    {
        //Width, Height of 1 tile
        public Vector2 UnitSize = Vector2.one;

        //Map size
        public Vector2 GridSize;

        //2-dimentional array of each tile(cell)
        public Building[,] Cells;

        public Transform trDecoRoot = null;

        // array of buildings categorized by building type
        public List<List<Building>> Buildings = new List<List<Building>>();

        protected override void Awake()
        {
            base.Awake();

            //Initialize Cells
            // if cell is blank(not occupied) value is null
            // otherwise, cell has script of occupying building
            Cells = new Building[(int)GridSize.x, (int)GridSize.y];
            for(int y = 0; y < GridSize.y; y++)
            {
                for(int x = 0; x < GridSize.x; x++)
                {
                    Cells[x, y] = null;
                }
            }

            // assume max building type count to 20
            for (int i = 0; i < 20; ++i)
            {
                Buildings.Add(new List<Building>());
            }

            
        }

        // get boundary of map
        public Vector2 GetBorder(Vector2 tileSize)
        {
            Vector2 vReturn = Vector2.zero;
            vReturn.x = (GridSize.x - tileSize.x) * -0.5f * UnitSize.x;
            vReturn.y = (GridSize.y - tileSize.y) * -0.5f * UnitSize.y;
            return vReturn;
        }

        // move gameobject to given tilepos and tile size(1x1,2x2,...)
        public void Move(GameObject go, Vector2 tilePos, Vector2 tileSize)
        {
            Vector2 border = GetBorder(tileSize);
            if (tilePos.x > GridSize.x - tileSize.x) tilePos.x = GridSize.x - tileSize.x;
            if (tilePos.y > GridSize.y - tileSize.y) tilePos.y = GridSize.y - tileSize.y;

            if (go != null)
            {
                Vector3 localPos = Vector3.zero;
                localPos.x = border.x + (int)(tilePos.x + 0.5f) * UnitSize.x;
                localPos.y = 0.01f;
                localPos.z = border.y + (int)(tilePos.y + 0.5f) * UnitSize.y;
                go.transform.position = localPos;
                go.transform.rotation = transform.rotation;
            }
        }

        // get tilepos(x,y coordinate index) from actual position and tilesize(1x1,2x2,...)
        public Vector2 GetTilePos(Vector3 vTarget, Vector2 tileSize)
        {
            Vector2 tilePos = Vector2.zero;
            Vector3 posLocal = vTarget;
            Vector2 border = GetBorder(tileSize);
            posLocal.x = Mathf.Clamp(posLocal.x, border.x, -border.x);
            posLocal.z = Mathf.Clamp(posLocal.z, border.y, -border.y);
            tilePos.x = (int)(posLocal.x - border.x) / (int)UnitSize.x;
            tilePos.y = (int)(posLocal.z - border.y) / (int)UnitSize.y;
            if (tilePos.x > GridSize.x - tileSize.x) tilePos.x = GridSize.x - tileSize.x;
            if (tilePos.y > GridSize.y - tileSize.y) tilePos.y = GridSize.y - tileSize.y;

            return tilePos;
        }

        // get actual position from tilepos(x,y coordinate index)
        public Vector3 TilePosToWorldPos(Vector2 tilePos)
        {

            Vector2 tileSize = Vector2.one;
            Vector2 border = GetBorder(tileSize);
            if (tilePos.x > GridSize.x - tileSize.x) tilePos.x = GridSize.x - tileSize.x;
            if (tilePos.y > GridSize.y - tileSize.y) tilePos.y = GridSize.y - tileSize.y;

            Vector3 localPos = Vector3.zero;
            localPos.x = border.x + (int)(tilePos.x + 0.5f) * UnitSize.x;
            localPos.y = 0.01f;
            localPos.z = border.y + (int)(tilePos.y + 0.5f) * UnitSize.y;

            return localPos;
        }

        // move building to proper position 
        // which has enough blank tiles to cover building's tilesize and nearest from current screen center
        // this function used when new building is created, set initial position to building
        public bool MoveToVacantTilePos(Building building)
        {
            List<Vector2> posTile = new List<Vector2>();

            for (int y = 0; y < (int)GridSize.y; ++y)
            {
                for (int x = 0; x < (int)GridSize.x; ++x)
                {

                    bool bOccupied = false;
                    for (int v = 0; v < (int)building.tileSize.y; ++v)
                    {
                        for (int u = 0; u < (int)building.tileSize.x; ++u)
                        {

                            if (x + u >= (int)GridSize.x) continue;
                            if (y + v >= (int)GridSize.y) continue;
                            if (Cells[x + u, y + v] != null)
                            {
                                bOccupied = true;
                                break;
                            }
                        }

                        if (bOccupied) break;
                    }

                    if (bOccupied)
                    {
                        continue;
                    }
                    else
                    {
                        // has enough blank tiles cover builsing's tile size
                        posTile.Add(new Vector2(x, y));
                    }
                }
            }

            // no vacant tilepos found
            if (posTile.Count == 0)
                return false;

            // get xzplane position with camera raycast 
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            float enter;
            GameSystem.instance.xzPlane.Raycast(ray, out enter);
            Vector3 vTarget = ray.GetPoint(enter);
            Vector2 TileCameraCenter = GetTilePos(vTarget, building.tileSize);

            // sort nearest tile from screen center
            int iFind = -1;
            float fDistMin = 0.0f;
            for (int j = 0; j < posTile.Count; ++j)
            {
                float fDist = Vector2.Distance(posTile[j], TileCameraCenter);
                if ((j == 0) || (fDist < fDistMin))
                {
                    iFind = j;
                    fDistMin = fDist;
                }
            }

            //iFind = 0;
            // sort listed point from camera canter to plane intersection
            building.tilePos = posTile[iFind];
            Move(building.gameObject, building.tilePos, building.tileSize);

            // if new building is outside of frustrum
            // move camera to show building
            Vector3 vPosCamera = TilePosToWorldPos(building.tilePos);
            GameObject.Find("CameraRoot").transform.position = vPosCamera;
            return true;
        }

        //check tile is vacant with given tilepos and tilesize
        public bool IsVacant(Vector2 tilePos, Vector2 tileSize)
        {
            for (int y = 0; y < (int)tileSize.y; ++y)
            {
                for (int x = 0; x < (int)tileSize.x; ++x)
                {
                    if (Cells[(int)tilePos.x + x, (int)tilePos.y + y] != null)
                        return false;
                }
            }

            return true;
        }

        // write tile cell was occupied by building
        public void OccupySet(Building building)
        {
            for (int y = 0; y < (int)building.tileSize.y; ++y)
            {
                for (int x = 0; x < (int)building.tileSize.x; ++x)
                {
                    Cells[(int)building.tilePos.x + x, (int)building.tilePos.y + y] = building.Landed ? building : null;
                }
            }
        }

        // get building with tile x, y coorsinate index
        public Building GetBuilding(int x, int y)
        {
            if ((x < 0) || ((int)GridSize.x <= x)) return null;
            if ((y < 0) || ((int)GridSize.y <= y)) return null;

            return Cells[x, y];
        }

        // get building count with given building type
        public int GetBuildingCount(int BuildingType)
        {
            return Buildings[BuildingType].Count;
        }

        // create new building with type and level
        public Building BuildingAdd(int type, int level)
        {
            // if previous selected building is exist, unselect that building
            // because newly created building must be in selection state
            if (GameSystem.instance.buildingSelected != null)
            {
                GameSystem.instance.BuildingLandUnselect();
            }

            // create building base from resource
            // each buildings are combination of buildingbase and building mesh
            GameObject goBuildingBase = GameSystem.instance.BuildingBase;
            GameObject go = (GameObject)Instantiate(goBuildingBase, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(trDecoRoot);

            Building building = go.GetComponent<Building>();
            building.ground = this;
            // initialize building
            building.Init(type, level);

            // add building to array
            Buildings[type].Add(building);

            return building;
        }

        // remove building
        public void BuildingRemove(Building building)
        {
            //int idx = Buildings[building.Type].FindIndex(x => x == building);
            //Debug.Log("idx:" + idx.ToString());
            //if (idx != -1)
            //{
            //    Buildings[building.Type].RemoveAt(idx);
            //}
        }
    }
}

