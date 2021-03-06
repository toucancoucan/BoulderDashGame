﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ClassLibrary.ConsoleInterface;
using ClassLibrary.DataLayer;
using ClassLibrary.InputProcessors;
using ClassLibrary.SoundPlayer;

namespace ClassLibrary {
    public class GameEngineConsole {
        private readonly AfterLevelScreen _afterLevelScreen = new AfterLevelScreen();
        private readonly DataLayer.DataLayer _dataLayer = new DataLayer.DataLayer();
        private readonly GameInputProcessor _gameInputProcessor = new GameInputProcessor();
        private readonly GameInterface _gameInterface = new GameInterface();
        private readonly GameLogic _gameLogic;
        private readonly Menu _menu = new Menu();
        private readonly MenuInputProcessor _menuInputProcessor = new MenuInputProcessor();
        private readonly int MenuItems = 6;

        private int _currentMenuAction;

        public GameEngineConsole() {
            _gameLogic = new GameLogic(ChangeGameStatus, () => _dataLayer, RefreshSaves,
                soundFilesEnum => { simulateSound(); });
        }

        private GameStatusEnum GameStatus { get; set; }

        //TODO: settings validation
        public Save NewGameSave { get; } = new Save();

        private void simulateSound() { }

        private void ChangeCurrentMenuAction(int i) {
            if (_currentMenuAction < MenuItems && _currentMenuAction >= 0) {
                _currentMenuAction += i;
                if (_currentMenuAction == MenuItems)
                    _currentMenuAction = 0;
                else if (_currentMenuAction == -1) _currentMenuAction = MenuItems - 1;
            }
        }

        private void PlaySound(SoundFilesEnum name) { }

        private void ChangeGameStatus(GameStatusEnum status) {
            GameStatus = status;
        }

        private void GraphicsThread() {
            var currentLevel = _gameLogic.CurrentLevel;
            var player = _gameLogic.Player;
            while (GameStatus == GameStatusEnum.Game) {
                _gameInterface.DrawUpperInterface(currentLevel.LevelName, player.Score,
                    currentLevel.GameMode.ToString());
                _gameInterface.DrawPlayerInterface(currentLevel.DiamondsQuantityToWin, player.CollectedDiamonds,
                    player.MaxEnergy, player.Energy, player.MaxHp, player.Hp, player.Name, player.Inventory);
                _gameInterface.NewDraw(() => currentLevel);
            }
            if (GameStatus == GameStatusEnum.WinScreen) _afterLevelScreen.DrawGameWin(player.Score, player.AllScores);
            else if (GameStatus == GameStatusEnum.LoseScreen) _afterLevelScreen.DrawGameLose();
        }

        private void GameLogicThread() {
            while (GameStatus == GameStatusEnum.Game) {
                Console.CursorVisible = false;
                Thread.Sleep(10000 / _dataLayer.Settings.TickRate);
                _gameLogic.GameLoop();
            }
        }

        private void InputThread() {
            while (GameStatus == GameStatusEnum.Game) {
                var c = Console.ReadKey(true);
                _gameInputProcessor.ProcessInput(c.Key, () => _gameLogic.Player, ChangeGameStatus);
            }
        }

        private void RefreshSaves() {
            _dataLayer.GetAllGameSaves();
        }
        public void Start() {
            RefreshSaves();
            MenuGameCycle();

            void MenuGameCycle() {
                try {
                    _menu.DrawMenu(_currentMenuAction);
                    while (GameStatus == GameStatusEnum.Menu) CreateMenu();
                    if (GameStatus == GameStatusEnum.Game) {
                        Console.Clear();
                        Parallel.Invoke(GraphicsThread, GameLogicThread, InputThread);
                    }
                    if (GameStatus == GameStatusEnum.WinScreen) {
                        _afterLevelScreen.DrawGameWin(_gameLogic.Player.Score, _gameLogic.Player.AllScores);
                        Console.ReadKey();
                        GameStatus = GameStatusEnum.Menu;
                    }
                    else if (GameStatus == GameStatusEnum.LoseScreen) {
                        _afterLevelScreen.DrawGameLose();
                        Console.ReadKey();
                        GameStatus = GameStatusEnum.Menu;
                    }
                    MenuGameCycle();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private void CreateMenu() {
            var c = Console.ReadKey(true);
            _menuInputProcessor.ProcessInput(
                c.Key,
                () => { Environment.Exit(0); },
                ChangeGameStatus,
                i => {
                    ChangeCurrentMenuAction(i);
                    _menu.DrawMenu(_currentMenuAction);
                },
                () => _currentMenuAction,
                () => {
                    _menu.DrawNewGame();
                    var name = Console.ReadLine();
                    _dataLayer.AddGameSave(new Save {Name = name});
                    var currentSave = _dataLayer.GetAllGameSaves().Find(e => e.Name == name);
                    if (currentSave == null) throw new Exception("Wrong save name!");
                    _gameLogic.CreateLevel(currentSave.LevelName, currentSave.Name, _dataLayer.Settings.SizeX,
                        _dataLayer.Settings.SizeY, _dataLayer.Settings.Difficulty, PlaySound);
                    _gameLogic.CurrentSave = currentSave;
                },
                () => {
                    _menu.DrawHelp();
                    Console.ReadKey();
                    _menu.DrawMenu(
                        4); //3 is "Help" index in _menuActions, so new  menu will be with this element current
                },
                () => {
                    _menu.DrawSettings(_dataLayer.Settings);
                    Console.WriteLine();
                    Console.Write(" New Difficulty: ");
                    _dataLayer.Settings.Difficulty = int.Parse(Console.ReadLine() ?? string.Empty);
                    Console.WriteLine();
                    Console.Write(" New TickRate: ");
                    _dataLayer.Settings.TickRate = int.Parse(Console.ReadLine() ?? string.Empty);
                    Console.WriteLine();
                    Console.Write(" New SizeX: ");
                    _dataLayer.Settings.SizeX = int.Parse(Console.ReadLine() ?? string.Empty);
                    Console.WriteLine();
                    Console.Write(" New SizeY: ");
                    _dataLayer.Settings.SizeY = int.Parse(Console.ReadLine() ?? string.Empty);
                    Console.WriteLine();
                    Console.Write(" Press any key to quit settings");
                    Console.ReadKey();
                    _dataLayer.SaveSettings();
                    _menu.DrawMenu(
                        2); //2 is "Settings" index in _menuActions, so new  menu will be with this element current
                },
                () => {
                    var results = _dataLayer.GetBestScores();
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
                    var saves = _dataLayer.GetAllGameSaves();
                    _menu.DrawSaves(saves);
                    var name = Console.ReadLine();
                    foreach (var save in saves)
                        if (save.Name == name) {
                            LaunchGame(save);
                            ChangeGameStatus(GameStatusEnum.Game);
                        }
                }
            );
        }

        private void LaunchGame(Save save) {
            _gameLogic.CreateLevel(save.LevelName, save.Name, _dataLayer.Settings.SizeX,
                _dataLayer.Settings.SizeY, _dataLayer.Settings.Difficulty, PlaySound);
            _gameLogic.Player.Score = save.Score;
            _gameLogic.Player.Hero = save.Hero;
            _gameLogic.CurrentSave = save;
            GameStatus = GameStatusEnum.Game;
        }
    }
}