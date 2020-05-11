﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ClassLibrary.ConsoleInterface;
using ClassLibrary.DataLayer;
using ClassLibrary.InputProcessors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ClassLibrary {
    public class GameEngine {
        private readonly Action _reDraw;
        
        public GameEngine(Action reDraw) {
            _reDraw = reDraw;
        }
        
        private const int
            GameLogicTickRate = 1; //TODO: carry it out in settings.
        private const int
            Fps = 30; //TODO: carry it out in settings.

        public int GameStatus; // 0 - menu; 1 - game; 2 - win screen; 3 - lose screen

        public int CurrentMenuAction=1;
        private readonly int MenuItems = 6;
        public bool isActionActive = false;

        public void ChangeGameStatus(int i) {
            if (i >= 0 && i < 4)
                GameStatus = i;
            else
                throw new Exception("Unknown game status");
        }

        public void ChangeCurrentMenuAction(int i) {
            if (CurrentMenuAction < MenuItems && CurrentMenuAction >= 0) {
                CurrentMenuAction += i;
                if (CurrentMenuAction == MenuItems)
                    CurrentMenuAction = 0;
                else if (CurrentMenuAction == -1) CurrentMenuAction = MenuItems - 1;
            }
        }

        public void PerformCurrentMenuAction() {
            switch (CurrentMenuAction) {
                case 0:
                    isActionActive = true;
                     break;
                case 1:
                    isActionActive = true;
                    break;
                case 2:
                    isActionActive = true;
                    break;
                case 3:
                    isActionActive = true;
                    break;
                case 4:
                    isActionActive = true;
                    break;
                case 5:
                    Environment.Exit(0);
                    break;
            }
        }

        private readonly Menu _menu = new Menu();
        public readonly GameLogic GameLogic = new GameLogic();
        public readonly DataInterlayer DataInterlayer = new DataInterlayer();
        private readonly MenuInputProcessor _menuInputProcessor = new MenuInputProcessor();
        // private readonly GameInterface _gameInterface = new GameInterface();
        // private readonly AfterLevelScreen _afterLevelScreen = new AfterLevelScreen();
        private readonly GameInputProcessor _gameInputProcessor = new GameInputProcessor();

        
        
        
        
        private void GraphicsThread() {

            #region Console drawing
            //Console
            // while (_gameStatus == 1) {
            //     _gameInterface.DrawUpperInterface(currentLevel.LevelName, player.Score, currentLevel.Aim);
            //     _gameInterface.DrawPlayerInterface(currentLevel.DiamondsQuantity, player.CollectedDiamonds,
            //         player.MaxEnergy, player.Energy, player.MaxHp, player.Hp, player.Name, player.Inventory);
            //     _gameInterface.NewDraw(() => currentLevel);
            // }
            // if (_gameStatus == 2) _afterLevelScreen.DrawGameWin(player.Score, player.AllScores);
            // else if (_gameStatus == 3) _afterLevelScreen.DrawGameLose();
            #endregion
            
            while (GameStatus == 1) {
                Thread.Sleep(1000/Fps);
                _reDraw();
            }
        }
        
        private void MenuGraphicsThread() {
            while (GameStatus == 0) {
                Thread.Sleep(1000/(Fps*2));
                _reDraw();
            }
        }

        private void GameLogicThread() {
            while (GameStatus == 1) {
                //Console.CursorVisible = false;
               Thread.Sleep(1000 / GameLogicTickRate);
                GameLogic.GameLoop();
            }
        }
        

        private void InputThread() {
            while (GameStatus == 1) {
                var c = Console.ReadKey(true);
                _gameInputProcessor.ProcessInput(c.Key, () => GameLogic.Player, ChangeGameStatus);
            }
        }

        public void Start() {
            Task musicPlayer = new Task(() => {
                SoundPlayer soundPlayer = new SoundPlayer();
                soundPlayer.playMusic();
            });
            //musicPlayer.Start(); //TODO: dont forget to enable music on build!

            //Console
            //_menu.DrawMenu(_currentMenuAction);

            //for develop
            GameStatus = 0;
            GameLogic.CreateLevel(-1, "testName", ChangeGameStatus, () => DataInterlayer);
            GameLogic.Player.Score = 100;
            GameLogic.CurrentSave = new Save();
            if (GameStatus == 0) {
                Parallel.Invoke(MenuGraphicsThread);
            }
            else if ( GameStatus == 1) {
                Parallel.Invoke(GraphicsThread,GameLogicThread); //and also GraphicsThread, , GameLogicThread , InputThread
            }
            // MenuGameCycle();
            //
            // void MenuGameCycle() {
            //     //Console
            //     //_menu.DrawMenu(_currentMenuAction);
            //     //while (_gameStatus == 0) CreateConsoleMenu();
            //     if (_gameStatus == 1) {
            //         //Console
            //         //Console.Clear();
            //         Parallel.Invoke(GraphicsThread, GameLogicThread); //and also , InputThread
            //     }
            //     MenuGameCycle();
            // }
        }

        private void CreateConsoleMenu() {
            var c = Console.ReadKey(true);
            _menuInputProcessor.ProcessInput(
                c.Key,
                () => { Environment.Exit(0); },
                ChangeGameStatus,
                i => {
                    ChangeCurrentMenuAction(i);
                    _menu.DrawMenu(CurrentMenuAction);
                },
                () => CurrentMenuAction,
                () => {
                    _menu.DrawNewGame();
                    var name = Console.ReadLine();
                    DataInterlayer.AddGameSave(name);
                    var currentSave = DataInterlayer.GetGameSaveByName(name);
                    GameLogic.CreateLevel(currentSave.LevelName, currentSave.Name, ChangeGameStatus,
                        () => DataInterlayer);
                    GameLogic.CurrentSave = currentSave;
                },
                () => {
                    _menu.DrawHelp();
                    Console.ReadKey();
                    _menu.DrawMenu(
                        4); //3 is "Help" index in _menuActions, so new  menu will be with this element current
                },
                () => {
                    _menu.DrawSettings();
                    Console.ReadKey();
                    _menu.DrawMenu(
                        2); //2 is "Settings" index in _menuActions, so new  menu will be with this element current
                },
                () => {
                    var results = DataInterlayer.GetBestScores();
                    _menu.DrawScores(results);
                    try {
                        Console.ReadKey();
                        _menu.DrawMenu(
                            3); //2 is "Settings" index in _menuActions, so new  menu will be with this element current
                    }
                    catch (Exception e) {
                        Console.WriteLine("Unable to read file with best scores");
                        Console.WriteLine(e.Message);
                    }
                },
                () => {
                    var saves = DataInterlayer.GetAllGameSaves();
                    _menu.DrawSaves(saves);
                    var name = Console.ReadLine();
                    foreach (var save in saves)
                        if (save.Name == name) {
                            ResumeGame(save);
                            ChangeGameStatus(1);
                        }
                }
            );
        }
        private void ResumeGame(Save save) {
            GameLogic.CreateLevel(save.LevelName, save.Name, ChangeGameStatus, () => DataInterlayer);
            GameLogic.Player.Score = save.Score;
            GameLogic.CurrentSave = save;
        }
    }
}