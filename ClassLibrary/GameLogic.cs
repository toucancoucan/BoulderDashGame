﻿using System;
using System.Collections.Generic;
using ClassLibrary.DataLayer;
using ClassLibrary.Entities;
using ClassLibrary.Entities.Enemies;
using ClassLibrary.Entities.Enemies.SmartEnemies;
using ClassLibrary.Entities.Expanding;
using ClassLibrary.Entities.Player;
using ClassLibrary.Matrix;
using ClassLibrary.SoundPlayer;

namespace ClassLibrary {
    public class GameLogic {
        private readonly Action<GameStatusEnum> _changeGameStatus;
        private readonly Func<DataLayer.DataLayer> _getDataLayer;
        private readonly Action<SoundFilesEnum> _playSound;
        private readonly Action _refreshEngineSaves;

        private int _chanceToDeleteAcidBlock;

        private int _chanceToSpawnEnemy;
        private int _difficulty;
        private bool _shouldUpdateSaves = true;
        public Save CurrentSave = null;

        public GameLogic(Action<GameStatusEnum> changeGameStatus,
            Func<DataLayer.DataLayer> getDataLayer,
            Action refreshEngineSaves, Action<SoundFilesEnum> playSound) {
            _changeGameStatus = changeGameStatus;
            _getDataLayer = getDataLayer;
            _refreshEngineSaves = refreshEngineSaves;
            _playSound = playSound;
        }
        public Level CurrentLevel { get; private set; }
        public Player Player { get; private set; }

        private void SetPlayer(Player pl) {
            Player = pl;
        }

        private void SubstractPlayerHp(int i) {
            Player.SubstractPlayerHp(i);
        }

        public void CreateLevel(int levelName, string playerName, int sizeX, int sizeY, int difficulty,
            Action<SoundFilesEnum> playSound
        ) {
            CurrentLevel = new Level(
                levelName, playerName,
                //() => CurrentLevel,
                Win,
                Lose,
                () => Player.PositionX,
                () => Player.PositionY,
                SubstractPlayerHp,
                SetPlayer,
                sizeX,
                sizeY,
                difficulty,
                playSound,
                () => {
                    _chanceToDeleteAcidBlock += 1;
                    CheckIfDeleteAllAcidBlocks();
                },
                () => Player
            );
            _difficulty = difficulty;
            _shouldUpdateSaves = true;
        }

        public void LoadLevel(int levelName, string playerName, int sizeX, int sizeY, int difficulty,
            Action<SoundFilesEnum> playSound, GameModesEnum mode, GameEntitiesEnum[,] map) {
            _difficulty = difficulty;
            CurrentLevel = new Level(
                levelName, playerName,
                //() => CurrentLevel,
                Win,
                Lose,
                () => Player.PositionX,
                () => Player.PositionY,
                SubstractPlayerHp,
                SetPlayer,
                sizeX,
                sizeY,
                difficulty,
                playSound,
                () => {
                    _chanceToDeleteAcidBlock += 1;
                    CheckIfDeleteAllAcidBlocks();
                },
                () => Player,
                mode,
                map
            );
            _shouldUpdateSaves = false;
        }

        public void GameLoop() {
            try {
                var used = new List<GameEntity>();
                for (var i = 0; i < CurrentLevel.Width; i++)
                for (var j = 0; j < CurrentLevel.Height; j++) {
                    if (used.Contains(CurrentLevel[i, j])) continue;
                    var tmp = CurrentLevel[i, j];
                    tmp.GameLoopAction();
                    used.Add(tmp);
                }

                SpawnEnemies();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }
        private void SpawnEnemies() {
            _chanceToSpawnEnemy += _difficulty;
            var randomX = Randomizer.Random(CurrentLevel.Width);
            var randomY = Randomizer.Random(CurrentLevel.Height);
            if (CurrentLevel[randomX, randomY].EntityEnumType != GameEntitiesEnum.EmptySpace ||
                _chanceToSpawnEnemy < Randomizer.Random(100)) return;

            var chanceToSpawn = Randomizer.Random(101);
            if (CurrentLevel._chanceToSpawnSmartDevil > chanceToSpawn) {
                CurrentLevel[randomX, randomY] =
                    new SmartDevil(randomX, randomY, () => CurrentLevel, () => Player.PositionX,
                        () => Player.PositionY, SubstractPlayerHp, () => Player, _playSound);
                _chanceToSpawnEnemy = 0;
            }

            else if (CurrentLevel._chanceToSpawnSmartSkeleton > chanceToSpawn) {
                CurrentLevel[randomX, randomY] =
                    new SmartSkeleton(randomX, randomY, () => CurrentLevel, () => Player.PositionX,
                        () => Player.PositionY, SubstractPlayerHp, () => Player, _playSound);
                _chanceToSpawnEnemy = 0;
            }
            else if (CurrentLevel._chanceToSpawnSmartPeaceful > chanceToSpawn) {
                CurrentLevel[randomX, randomY] =
                    new SmartPeaceful(randomX, randomY, () => CurrentLevel, () => Player.PositionX,
                        () => Player.PositionY, SubstractPlayerHp, () => Player, _playSound);
                _chanceToSpawnEnemy = 0;
            }
            else if (CurrentLevel._chanceToSpawnSmartDigger > chanceToSpawn) {
                CurrentLevel[randomX, randomY] =
                    new EnemyDigger(randomX, randomY, () => CurrentLevel, () => Player.PositionX,
                        () => Player.PositionY, SubstractPlayerHp);
                _chanceToSpawnEnemy = 0;
            }
            else {
                CurrentLevel[randomX, randomY] =
                    new EnemyWalker(randomX, randomY, () => CurrentLevel, () => Player.PositionX,
                        () => Player.PositionY, SubstractPlayerHp);
                _chanceToSpawnEnemy = 0;
            }
        }

        private void CheckIfDeleteAllAcidBlocks() {
            if (_chanceToDeleteAcidBlock >= 50) {
                for (var i = 0; i < CurrentLevel.Width; i++)
                for (var j = 0; j < CurrentLevel.Height; j++)
                    if (CurrentLevel[i, j] is Acid)
                        ((Acid) CurrentLevel[i, j]).TurnIntoRock();
                _chanceToDeleteAcidBlock = 0;
            }
        }
        private void Win() {
            _changeGameStatus(GameStatusEnum.WinScreen);
            if (!_shouldUpdateSaves) return;
            var dataInterlayer = _getDataLayer();
            CurrentSave.LevelName = CurrentLevel.LevelName;
            CurrentSave.Score = Player.Score;
            CurrentSave.LevelName += 1;
            dataInterlayer.ChangeGameSave(CurrentSave);
            _refreshEngineSaves();
        }

        private void Lose() {
            _changeGameStatus(GameStatusEnum.LoseScreen);
            if (!_shouldUpdateSaves) return;
            var dataInterlayer = _getDataLayer();
            dataInterlayer.AddBestScore(Player.Name, Player.Score);
            dataInterlayer.DeleteGameSave(CurrentSave);
            _refreshEngineSaves();
        }
    }
}