﻿using System;
using ClassLibrary.Entities.Basic;
using ClassLibrary.Matrix;

namespace ClassLibrary.Entities.Enemies {
    public class Rock : Enemy {
        private bool _isFalling;
        public Rock(int i, int j, Func<Level> getLevel,
            Action<int> changePlayerHp
        ) : base(i, j, getLevel, changePlayerHp) {
            EntityEnumType = GameEntitiesEnum.Rock;
            Damage = 4;
            CanMove = false;
            Hp = 1000;
            MoveWeight = 200;
        }
        public override void GameLoopAction() {
            RockFall();
            RockDamage();
        }

        private void RockFall() {
            var currentLevel = GetLevel();
            _isFalling = false;

            if (PositionX + 1 >= currentLevel.Width ||
                currentLevel[PositionX + 1, PositionY].EntityEnumType != GameEntitiesEnum.EmptySpace ||
                _isFalling) return;
            Move(MoveDirectionEnum.Horizontal, 1);
            _isFalling = true;
        }

        public override void BreakAction(Player.Player player) {
            var currentLevel = GetLevel();
            if (player.PositionY < PositionY && currentLevel[PositionX, PositionY + 1] is EmptySpace)
                Move(MoveDirectionEnum.Vertical, 1);
            else if (currentLevel[PositionX, PositionY - 1] is EmptySpace) Move(MoveDirectionEnum.Vertical, -1);
        }

        private void RockDamage() {
            if (PositionX + 1 == GetLevel().Width) return;
            if (_isFalling && GetLevel()[PositionX + 1, PositionY].EntityEnumType == GameEntitiesEnum.Player)
                DealDamage();
        }
    }
}