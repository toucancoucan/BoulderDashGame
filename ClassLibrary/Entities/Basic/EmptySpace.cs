﻿namespace ClassLibrary.Entities.Basic {
    public class EmptySpace : GameEntity {
        public EmptySpace(int i, int j) : base(i, j) {
            EntityEnumType = GameEntitiesEnum.EmptySpace;
        }
    }
}