﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UnitHarvester : Unit {

    // Each frame the unit is harvesting harvestAmount increases
    // When this reaches harvestAmountPerResource resources are gained
    [ReadOnly] public float harvestAmount;
    private const float harvestAmountPerResource = 0.1f;
    private const float harvestSpeed = 0.25f;

    private ResourceObject resourceToHarvest;
    private bool isHarvesting;

    protected override void Update() {
        base.Update();

        if(resourceToHarvest != null) {
            float distToResource = Vector2Int.Distance(tile.pos, resourceToHarvest.tile.pos);
            if(distToResource <= 1 && movement.hasReachedDestination && resourceToHarvest.resourceAmount > 0) {
                movement.LookAtTile(resourceToHarvest.tile);

                harvestAmount += Time.deltaTime * harvestSpeed;
                if(harvestAmount >= harvestAmountPerResource) {
                    harvestAmount = 0;
                    resourceToHarvest.Harvest(harvestAmountPerResource);
                    if(resourceToHarvest.type == ResourceType.Gem) GameManager.instance.AddGems(5);
                    if(resourceToHarvest.type == ResourceType.Mineral) GameManager.instance.AddMinerals(20);
                    if(GameManager.instance.IsAtMaxResource(resourceToHarvest.type)) {
                        cancelHarvesting();
                        return;
                    }
                }
                isHarvesting = true;
                if(!resourceToHarvest.unitsHarvesting.Contains(this)) resourceToHarvest.unitsHarvesting.Add(this);
            } else if(isHarvesting) {
                cancelHarvesting();
            }
        }
    }

    public void HarvestResource(ResourceObject resource) {
        if(resource == resourceToHarvest || GameManager.instance.IsAtMaxResource(resource.type)) return;
        cancelHarvesting();

        TileData[] freeTiles = resource.tile.connections.Where(o => (o.occupiedUnit == null || o.occupiedUnit == this) && o.IsTileAccessible(movement.isFlying)).ToArray();
        freeTiles = freeTiles.OrderBy(o => Vector3.Distance(o.worldPos, transform.position)).ToArray();

        if(freeTiles.Length > 0) {
            MoveToPoint(freeTiles[0]);
            resourceToHarvest = resource;
        }
    }

    public override void MoveToPoint(TileData tile) {
        base.MoveToPoint(tile);
        cancelHarvesting();
    }

    private void cancelHarvesting() {
        if(resourceToHarvest && resourceToHarvest.unitsHarvesting.Contains(this)) resourceToHarvest.unitsHarvesting.Remove(this);
        resourceToHarvest = null;
        isHarvesting = false;
    }
}
