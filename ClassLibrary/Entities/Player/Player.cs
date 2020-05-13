﻿using System;
using System.Collections.Generic;
using ClassLibrary.Entities.Basic;
using ClassLibrary.Entities.Collectable;
using ClassLibrary.Entities.Collectable.ItemsTiles;
using ClassLibrary.Entities.Enemies;
using ClassLibrary.Entities.Expanding;
using ClassLibrary.Matrix;

namespace ClassLibrary.Entities.Player {
    public class Player : Movable {
        private readonly int _attackEnergyCost = 6;
        private readonly int _diamondsTowWin;
        private readonly Action _lose;
        private readonly int _moveEnergyCost = 1;
        private readonly int _moveRockEnergyCost = 5;
        private readonly Action _win;
        public readonly Dictionary<string, int[]> AllScores;
        public readonly Inventory Inventory = new Inventory();
        public readonly Keyboard Keyboard = new Keyboard();
        private readonly int MaxEnergy = 20;
        public readonly string Name;

        public Player(
            int i,
            int j,
            string name,
            Func<Level> getLevel,
            Action win,
            Action lose,
            int diamondsTowWin)
            : base(getLevel, i, j) {
            Name = name;
            _win = win;
            _lose = lose;
            Hp = MaxHp;
            _diamondsTowWin = diamondsTowWin;
            EntityType = 0;
            AllScores = new Dictionary<string, int[]> {
                {"Collected diamonds", new[] {0, 0}},
                {"Collected lucky boxes", new[] {0, 0}},
                {"Diamonds from lucky box", new[] {0, 0}},
                {"Score from lucky box", new[] {0, 0}}
            };
            CanMove = false;

            //TODO: delete thiS features (its only for testing)
            PlayerAnimator = new PlayerAnimator(1);
            Inventory.ArmorLevel = 5;
            Inventory.SwordLevel = 5;
            Inventory.TntQuantity = 5;
            Inventory.StoneInDiamondsConverterQuantity = 5;
        }
        public int MaxHp { get; set; } = 10;
        public int Energy { get; private set; } = 20;
        public int CollectedDiamonds { get; set; }
        public int EnergyRestoreTick { get; set; } = 1;
        public int ScoreMultiplier { get; set; } = 10;
        public int Score { get; set; }
        public int Hero { get; set; }
        public PlayerAnimator PlayerAnimator { get; }

        public int GetScoreToAdd(int value) {
            return value * ScoreMultiplier;
        }
        //TODO: add scores for picking inventory and killing enemies
        public void AddScore(int value) {
            Score += value * ScoreMultiplier;
        }

        public new void GameLoopAction() {
            CheckWin();
            CheckLose();
            RestoreEnergy();
        }

        public void SubstractPlayerHp(int value) {
            if (Inventory.ArmorLevel > 0) {
                Inventory.ArmorCellHp -= value;
                value -= Inventory.ArmorLevel;
            }
            if (Inventory.ArmorCellHp <= 0) {
                Inventory.ArmorLevel--;
                Inventory.ArmorCellHp = 10;
            }
            if (value > 0) Hp -= value;
            SetAnimation(2);
        }

        private void SubstractPlayerHp(int value, int animation) {
            if (Inventory.ArmorLevel > 0) {
                Inventory.ArmorCellHp -= value;
                value -= Inventory.ArmorLevel;
            }
            if (Inventory.ArmorCellHp <= 0) {
                Inventory.ArmorLevel--;
                Inventory.ArmorCellHp = 10;
            }
            if (value > 0) Hp -= value;
            SetAnimation(animation);
        }

        public void Move(string direction, int value) {
            if (!EnoughEnergy()) return;
            var willMove = false;
            var level = GetLevel();
            level[PositionX, PositionY] = new EmptySpace(PositionX, PositionY);
            switch (direction) {
                case "vertical":
                    PositionX += value;
                    if (CheckNewPosition(level))
                        PositionX -= value;
                    else
                        willMove = true;
                    break;
                case "horizontal":
                    PositionY += value;
                    if (level[PositionX, PositionY].EntityType == 3 && EnoughEnergyForRock()) {
                        ((Rock) level[PositionX, PositionY]).PushRock(PositionX, PositionY, "horizontal", value);
                        Energy -= _moveRockEnergyCost;
                    }
                    if (CheckNewPosition(level))
                        PositionY -= value;
                    else
                        willMove = true;
                    break;
                default:
                    throw new Exception("Unknown move direction in Player.cs");
            }
            if (willMove) {
                Energy -= _moveEnergyCost;
                switch (level[PositionX, PositionY].EntityType) {
                    case 4:
                        ((Diamond) level[PositionX, PositionY]).Collect(() => this);
                        break;
                    case 7:
                        ((LuckyBox) level[PositionX, PositionY]).Collect(() => this);
                        break;
                    case 12:
                        ((BarrelWithSubstance) level[PositionX, PositionY]).Collect(GetLevel, SubstractPlayerHp);
                        break;
                    case 20:
                        ((SwordTile) level[PositionX, PositionY]).Collect(() => Inventory);
                        break;
                    case 21:
                        ((ConverterTile) level[PositionX, PositionY]).Collect(() => Inventory);
                        break;
                    case 22:
                        ((TntTile) level[PositionX, PositionY]).Collect(() => Inventory);
                        break;
                    case 23:
                        ((ArmorTile) level[PositionX, PositionY]).Collect(() => Inventory);
                        break;
                }
            }
            level[PositionX, PositionY] = this;
        }
        private bool CheckNewPosition(Level level) {
            return PositionX == level.Width || PositionX == -1 || PositionY == level.Height || PositionY == -1 ||
                   level[PositionX, PositionY].CanMove == false;
        }
        private bool EnoughEnergyForRock() {
            return Energy >= _moveRockEnergyCost;
        }
        private bool EnoughEnergy() {
            return Energy >= _moveEnergyCost;
        }
        public void HpInEnergy() {
            SubstractPlayerHp(1);
            Energy = MaxEnergy;
        }
        public void Teleport() {
            if (Energy < MaxEnergy) return;
            SetAnimation(5);
            var level = GetLevel();
            Energy = 0;
            var posX = Randomizer.Random(level.Width);
            var posY = Randomizer.Random(level.Height);
            level[PositionX, PositionY] = new EmptySpace(PositionX, PositionY);
            PositionX = posX;
            PositionY = posY;
            level[PositionX, PositionY] = this;
        }
        public void ConvertNearStonesInDiamonds() {
            if (Inventory.StoneInDiamondsConverterQuantity == 0) return;
            SetAnimation(6);
            Inventory.StoneInDiamondsConverterQuantity--;
            var level = GetLevel();
            if (PositionX + 1 < level.Width && level[PositionX + 1, PositionY].EntityType == 3) {
                var tmp = new StoneInDiamondConverter(PositionX + 1, PositionY, GetLevel);
                level[PositionX + 1, PositionY] = tmp;
            }
            if (PositionX - 1 >= 0 && level[PositionX - 1, PositionY].EntityType == 3) {
                var tmp = new StoneInDiamondConverter(PositionX - 1, PositionY, GetLevel);
                level[PositionX - 1, PositionY] = tmp;
            }
            if (PositionY + 1 < level.Height && level[PositionX, PositionY + 1].EntityType == 3) {
                var tmp = new StoneInDiamondConverter(PositionX, PositionY + 1, GetLevel);
                level[PositionX, PositionY + 1] = tmp;
            }
            if (PositionY - 1 >= 0 && level[PositionX, PositionY - 1].EntityType == 3) {
                var tmp = new StoneInDiamondConverter(PositionX, PositionY - 1, GetLevel);
                level[PositionX, PositionY - 1] = tmp;
            }
            Energy = Energy / 2;
        }
        public void Attack() {
            if (Energy < _attackEnergyCost) return;
            SetAnimation(3);
            Energy -= _attackEnergyCost;
            var level = GetLevel();
            Enemy tmp = null;
            if (Right < level.Width && level[Right, PositionY] is Enemy) {
                tmp = (Enemy) level[Right, PositionY];
                tmp.Hp -= Inventory.SwordLevel;
            }
            else if (Left >= 0 && level[Left, PositionY] is Enemy) {
                tmp = (Enemy) level[Left, PositionY];
                tmp.Hp -= Inventory.SwordLevel;
            }
            else if (Bot < level.Height && level[PositionX, Bot] is Enemy) {
                tmp = (Enemy) level[PositionX, Bot];
                tmp.Hp -= Inventory.SwordLevel;
            }
            else if (Top >= 0 && level[PositionX, Top] is Enemy) {
                tmp = (Enemy) level[PositionX, Top];
                tmp.Hp -= Inventory.SwordLevel;
            }
            if (tmp != null && tmp.Hp <= 0) level[tmp.PositionX, tmp.PositionY] = new Diamond(PositionX, PositionY);
        }
        public void UseTnt() {
            if (Inventory.TntQuantity == 0) return;
            Inventory.TntQuantity--;
            var level = GetLevel();
            double dmg = 0;
            var tileDamage = 0.9;

            if (Right < level.Width) {
                level[Right, PositionY] = new EmptySpace(Right, PositionY);
                dmg += tileDamage;
            }
            if (Left >= 0) {
                level[Left, PositionY] = new EmptySpace(Left, PositionY);
                dmg += tileDamage;
            }
            if (Bot < level.Height) {
                level[PositionX, Bot] = new EmptySpace(PositionX, Bot);
                dmg += tileDamage;
            }
            if (Top >= 0) {
                level[PositionX, Top] = new EmptySpace(PositionX, Top);
                dmg += tileDamage;
            }

            if (Right < level.Width && Bot < level.Height) {
                level[Right, Bot] = new EmptySpace(Right, Bot);
                dmg += tileDamage;
            }
            if (Right < level.Width && Top >= 0) {
                level[Right, Top] = new EmptySpace(Right, Top);
                dmg += tileDamage;
            }
            if (Left >= 0 && Bot < level.Height) {
                level[Left, Bot] = new EmptySpace(Left, Bot);
                dmg += tileDamage;
            }
            if (Left >= 0 && Top >= 0) {
                level[Left, Top] = new EmptySpace(Left, Top);
                dmg += tileDamage;
            }

            Energy = Energy / 2;
            SubstractPlayerHp(Convert.ToInt32(dmg), 4);
        }
        private void CheckLose() {
            if (Hp <= 0)
                _lose();
        }
        private void CheckWin() {
            if (CollectedDiamonds >= _diamondsTowWin) //TODO: must change depending on level
                _win();
        }
        private void RestoreEnergy() {
            if (Energy < MaxEnergy) Energy += EnergyRestoreTick;
        }

        public void SetAnimation(int currentAnimation) {
            PlayerAnimator.SetAnimation(currentAnimation);
        }
    }
}