﻿namespace ClassLibrary.Entities.Basic {
    public class Wall : GameEntity {
        public Wall(int i, int j) : base(i, j) {
            EntityEnumType = GameEntitiesEnum.Wall;
            CanMove = false;
            MoveWeight = 300;
        }
    }
}