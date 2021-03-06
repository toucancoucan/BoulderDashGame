﻿using System;
using ClassLibrary.Entities.Enemies;
using ClassLibrary.Matrix;

namespace ClassLibrary.Entities.Expanding {
    public class Acid : Expandable {
        private readonly Action _acidGameLoopAction;
        private readonly Action<int> _changePlayerHp;
        private int _actionsCounter;

        public Acid(int i, int j, Func<Level> getLevel, Action<int> changePlayerHp, Action acidGameLoopAction) : base(i,
            j, getLevel) {
            EntityEnumType = GameEntitiesEnum.Acid;
            _changePlayerHp = changePlayerHp;
            _acidGameLoopAction = acidGameLoopAction;
            CanMove = false;
            ConstructorForExpand = (i, j) => {
                var level = GetLevel();
                var tmp = new Acid(i, j, GetLevel, _changePlayerHp, _acidGameLoopAction);
                level[i, j] = tmp;
            };
        }

        public override void GameLoopAction() {
            if (_actionsCounter == 3) Expand((i, j) => GetLevel()[i, j].CanMove, ConstructorForExpand);
            if (_actionsCounter >= 4) {
                TurnIntoRock();
                _actionsCounter = 0;
            }
            _actionsCounter++;
            _acidGameLoopAction();
        }

        public void TurnIntoRock() {
            var level = GetLevel();
            level[PositionX, PositionY] = new Rock(PositionX, PositionY, GetLevel, _changePlayerHp);
        }
    }
}