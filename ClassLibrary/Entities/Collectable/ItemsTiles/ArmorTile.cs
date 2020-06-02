﻿using System;
using ClassLibrary.Entities.Player;

namespace ClassLibrary.Entities.Collectable.ItemsTiles {
    public class ArmorTile : ItemCollectible {
        public ArmorTile(int i, int j) : base(i, j) {
            EntityEnumType = GameEntitiesEnum.ArmorTile;
        }

        public void Collect(Func<Inventory> getPlayerInventory) {
            getPlayerInventory().ArmorLevel++;
        }
    }
}