﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : Singleton<World> {

    public static float tileSize = 1.0f;

    public WorldGenerator generator;
    public Pathfinder pathfinder;
    public WorldSurface surface;
    public Transform unitContainer;

    public TileObject tilePrefab;
    public TileObject tileEdgePrefab;
    public WorldObject basePrefab;
    public WorldObject minePrefab;
    public WorldObject mineralPrefab;
    public Unit unitarvesterPrefab, unitScoutPrefab;

    [HideInInspector] public TileData[,] tileDataMap;
    [HideInInspector] public TileObject[,] tileObjectMap;
    [HideInInspector] public List<TileObject> allTiles = new List<TileObject>();
    
    private List<Unit> units = new List<Unit>();

    protected override void Awake() {
        Build();
        SpawnUnit(unitarvesterPrefab, 54, 54);
        SpawnUnit(unitScoutPrefab, 56, 54);
    }

    public void Build() {
        tileDataMap = generator.Generate();
        // Destroy existing tiles
        clear();

        tileObjectMap = new TileObject[generator.worldSize, generator.worldSize];

        // Spawn tile objects
		for (int j = 0; j < generator.worldSize; j++) {
			for (int i = 0; i < generator.worldSize; i++) {
                if(tileDataMap[i, j].type == TileType.None) continue;
                // The sides of edge tiles are visible so a more complex model is needed, non-edge tiles can just be a plane
                bool tileUp = generator.IsInBounds(i, j + 1) && tileDataMap[i, j + 1].type != 0;
                bool tileDown = generator.IsInBounds(i, j - 1) && tileDataMap[i, j - 1].type != 0;
                bool tileLeft = generator.IsInBounds(i - 1, j) && tileDataMap[i - 1, j].type != 0;
                bool tileRight = generator.IsInBounds(i + 1, j) && tileDataMap[i + 1, j].type != 0;
                bool isEdgeTile = !tileUp || !tileDown || !tileLeft || !tileRight;

                TileObject tile = Instantiate(isEdgeTile ? tileEdgePrefab : tilePrefab, GetTilePos(i, j), Quaternion.identity, transform);
                tile.tileData = tileDataMap[i, j];
                tile.name += " (" + i + ", " + j + ")";
                tileObjectMap[i, j] = tile;
                allTiles.Add(tile);

                WorldObject worldObject = null;

                switch(tileDataMap[i, j].type) {
                    case TileType.Base:
                        worldObject = Instantiate(basePrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<WorldObject>();
                        break;
                    case TileType.Mine:
                        worldObject = Instantiate(minePrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<WorldObject>();
                        break;
                    case TileType.Mineral:
                        worldObject = Instantiate(mineralPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<WorldObject>();
                        break;
                }

                if(worldObject != null) {
                    worldObject.transform.SetParent(tile.transform, false);
                    worldObject.tile = tile.tileData;
                }
			}
		}
    }

    public void SpawnUnit(Unit prefab, int i, int j) {
        Unit unit = Instantiate(prefab.gameObject, GetTilePos(i, j), Quaternion.identity, unitContainer).GetComponent<Unit>();
        unit.tile = tileDataMap[i, j];
        units.Add(unit);
    }

    public Vector3 GetTilePos(int i, int j) {
        return new Vector3(-generator.worldSize * tileSize / 2 + tileSize * i, 0, -generator.worldSize * tileSize / 2 + tileSize * j);
    }

    public Vector3 GetTilePos(TileData tile) {
        return GetTilePos(tile.i, tile.j);
    }

    private void clear() {
        foreach(TileObject tile in allTiles) Destroy(tile.gameObject);
        foreach(Unit unit in units) Destroy(unit);
        allTiles.Clear();
        units.Clear();
    }
}
