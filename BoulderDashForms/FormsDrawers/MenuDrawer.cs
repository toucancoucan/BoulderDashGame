﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using ClassLibrary;
using ClassLibrary.Entities;

namespace BoulderDashForms.FormsDrawers {
    public class MenuDrawer : FormDrawer {
        private readonly string[] _menuActions = {"Continue", "New game", "Settings", "Scores", "Help", "Exit"};
        private readonly int maxFramesForEnemies = 4;
        private int _currentFrameForEnemies;
        private int _enemyPosition;
        private SortedDictionary<int, string> _results;
        private int _rightBlockWidth = 1000;

        public MenuDrawer() {
            FontCollection = new PrivateFontCollection();
            FontCollection.AddFontFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, @"Fonts\monogram.ttf"));
            FontCollection.AddFontFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, @"Fonts\ThaleahFat.ttf"));
            FontCollection.AddFontFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, @"Fonts\m5x7.ttf"));
            Family1 = FontCollection.Families[0];
            Family2 = FontCollection.Families[1];
            var family3 = FontCollection.Families[2];
            MenuFont = new Font(Family1, 28);
            BoldFont = new Font(family3, 28);
            HeaderFont = new Font(family3, 38);
            MainFont = new Font(Family2, 30);
        }
        public void NullRightBlockWidth() {
            _rightBlockWidth = 0;
        }

        public void DrawMenu(Graphics graphics, GameEngine gameEngine) {
            FillBackground(graphics);
            FrontSpritesAnimation(graphics);
            DrawMainMenuBlock(graphics, gameEngine);
            DrawRightBlock(graphics, gameEngine);
            DrawRightBlockContent(graphics, gameEngine);
        }
        private void DrawRightBlockContent(Graphics graphics, GameEngine gameEngine) {
            if (_rightBlockWidth < 1000) return;
            var blockHeader = "";
            switch (gameEngine.CurrentMenuAction) {
                case 0:
                    blockHeader = "Select save";
                    DrawContinue(graphics, gameEngine);
                    break;
                case 1:
                    blockHeader = "Create new game";
                    DrawNewGame(graphics, gameEngine);
                    break;
                case 2:
                    blockHeader = "Adjust your settings";
                    DrawSettings(graphics, gameEngine);
                    break;
                case 3:
                    blockHeader = "Best results";
                    DrawBestScores(graphics, gameEngine);
                    break;
                case 4:
                    blockHeader = "Game guide";
                    DrawHelp(graphics);
                    break;
                case 5:
                    blockHeader = "Press ENTER to exit game";
                    break;
            }
            graphics.DrawString(blockHeader, HeaderFont, WhiteBrush, 520, 160);
        }
        private void DrawNewGame(Graphics graphics, GameEngine gameEngine) {
            var selected = new Rectangle(520, 220 + gameEngine.CurrentSubAction * 60, 940, 40);
            graphics.FillRectangle(gameEngine.IsNameEntered == false ? DarkBrush : RedBrush, selected);
            var hero = 36;
            switch (gameEngine.NewGameSave.Hero) {
                case 0:
                    hero = 4;
                    break;
                case 1:
                    hero = 36;
                    break;
                case 2:
                    hero = 68;
                    break;
                case 3:
                    hero = 100;
                    break;
                case 4:
                    hero = 132;
                    break;
                case 5:
                    hero = 164;
                    break;
            }
            const int kf = 3;
            var pixelY = hero + kf;
            var destRect =
                new Rectangle(new Point(820, 210),
                    new Size(32, 52));
            var srcRect = new Rectangle(new Point(9 * 16, pixelY),
                new Size(16, 26));
            graphics.DrawImage(MainSprites, destRect, srcRect, GraphicsUnit.Pixel);

            graphics.DrawString("Choose your hero: ",
                MainFont, WhiteBrush, 520, 220);
            graphics.DrawString($"Enter your name: {gameEngine.NewGameSave.Name}",
                MainFont, WhiteBrush, 520, 280);
            graphics.DrawString("Start game",
                MainFont, WhiteBrush, 520, 340);
        }
        private void DrawRightBlock(Graphics graphics, GameEngine gameEngine) {
            var rightBlock = new Rectangle(500, 150, _rightBlockWidth, 600);
            graphics.FillRectangle(gameEngine.IsActionActive ? RedBrushHalfTransparent : RedBrushTransparent,
                rightBlock);
            if (_rightBlockWidth < 1000) _rightBlockWidth += 100;
        }
        private void DrawMainMenuBlock(Graphics graphics, GameEngine gameEngine) {
            var rect = new Rectangle(150, 0, 350, 900);
            graphics.FillRectangle(DarkBrush, rect);
            graphics.DrawString("DUNGEON", HeaderFont, RedBrush, 160, 30);
            graphics.DrawString("DASH", HeaderFont, RedBrush, 160, 68);
            for (var i = 0; i < _menuActions.Length; i++)
                if (gameEngine.CurrentMenuAction == i)
                    graphics.DrawString(_menuActions[i], MainFont, WhiteBrush, 170, 200 + i * 36);
                else
                    graphics.DrawString(_menuActions[i], MainFont, RedBrush, 160, 200 + i * 36);
        }
        private void DrawContinue(Graphics graphics, GameEngine gameEngine) {
            var counter = 1;
            var selected = new Rectangle(520, 220 + gameEngine.CurrentSubAction * 40, 940, 40);
            graphics.FillRectangle(DarkBrush,
                selected);
            foreach (var result in gameEngine.Saves) {
                graphics.DrawString($" Name: {result.Name}",
                    MainFont, WhiteBrush, 520, 180 + 40 * counter);
                graphics.DrawString($"Score: {result.Score}",
                    MainFont, WhiteBrush, 910, 180 + 40 * counter);
                graphics.DrawString($"Level: {result.LevelName}",
                    MainFont, WhiteBrush, 1200, 180 + 40 * counter);
                counter++;
            }
        }
        private void DrawHelp(Graphics graphics) {
            graphics.DrawString("Use W,A,S,D to move hero", MainFont, WhiteBrush, 650, 226);
            var destRect =
                new Rectangle(new Point(550, 210),
                    new Size(32, 32));
            var srcRect = new Rectangle(new Point(4 * 16, 2 * 16), new Size(16, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);
            //a
            destRect =
                new Rectangle(new Point(518, 242),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(3 * 16, 3 * 16), new Size(16, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);

            //s
            destRect =
                new Rectangle(new Point(550, 242),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(4 * 16, 3 * 16), new Size(16, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);
            //d
            destRect =
                new Rectangle(new Point(582, 242),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(5 * 16, 3 * 16), new Size(16, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);

            graphics.DrawString("Use SPACE to restore energy, Q,E,R,T for abilities", MainFont, WhiteBrush, 650, 326);
            //space
            destRect =
                new Rectangle(new Point(518, 348),
                    new Size(126, 32));
            srcRect = new Rectangle(new Point(5 * 16, 5 * 16), new Size(80, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);
            //t
            destRect =
                new Rectangle(new Point(614, 306),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(7 * 16, 2 * 16), new Size(16, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);
            //q
            destRect =
                new Rectangle(new Point(518, 306),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(3 * 16, 2 * 16), new Size(16, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);
            //e
            destRect =
                new Rectangle(new Point(550, 306),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(5 * 16, 2 * 16), new Size(16, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);
            //r
            destRect =
                new Rectangle(new Point(582, 306),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(6 * 16, 2 * 16), new Size(16, 16));
            graphics.DrawImage(Keyboard, destRect, srcRect, GraphicsUnit.Pixel);

            graphics.DrawString("Collect diamonds to complete level", MainFont, WhiteBrush, 650, 406);
            destRect =
                new Rectangle(new Point(550, 406),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(1 * 16, 7 * 16), new Size(15, 16));
            graphics.DrawImage(Icons, destRect, srcRect, GraphicsUnit.Pixel);
            graphics.DrawString("Avoid enemies...or fight with them", MainFont, WhiteBrush, 650, 476);
            destRect =
                new Rectangle(new Point(550, 476),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(23 * 16, 5 * 16), new Size(16, 16));
            graphics.DrawImage(MainSprites, destRect, srcRect, GraphicsUnit.Pixel);
            graphics.DrawString("Explore different game mechanics and features", MainFont, WhiteBrush, 650, 556);
            destRect =
                new Rectangle(new Point(550, 556),
                    new Size(32, 32));
            srcRect = new Rectangle(new Point(14 * 16, 13 * 16), new Size(15, 16));
            graphics.DrawImage(MainSprites, destRect, srcRect, GraphicsUnit.Pixel);
        }
        private void FrontSpritesAnimation(Graphics graphics) {
            var destination =
                new Rectangle(new Point(0 + _enemyPosition, 810),
                    new Size(32, 32));
            var res = new Rectangle(new Point(12 * 16 + _currentFrameForEnemies * 16, 74), new Size(16, 21));
            graphics.DrawImage(MainSprites, destination, res, GraphicsUnit.Pixel);
            destination =
                new Rectangle(new Point(0 + _enemyPosition - 72, 810),
                    new Size(32, 32));
            res = new Rectangle(new Point(27 * 16 + _currentFrameForEnemies * 16, 5 * 16), new Size(16, 16));
            graphics.DrawImage(MainSprites, destination, res, GraphicsUnit.Pixel);

            _enemyPosition += 3;
            if (_enemyPosition >= 1500)
                _enemyPosition = 0;

            if (_currentFrameForEnemies < maxFramesForEnemies - 1)
                _currentFrameForEnemies++;
            else
                _currentFrameForEnemies = 0;
        }
        private void FillBackground(Graphics graphics) {
            for (var i = 0; i < 31; i++)
            for (var j = 0; j < 53; j++) {
                var destRect =
                    new Rectangle(new Point(j * GameEntity.FormsSize * 2, i * GameEntity.FormsSize * 2),
                        new Size(GameEntity.FormsSize * 2, GameEntity.FormsSize * 2));
                var srcRect = new Rectangle(new Point(1 * 16, 1 * 16), new Size(16, 16));
                graphics.DrawImage(MainSprites, destRect, srcRect, GraphicsUnit.Pixel);
                if (Randomizer.Random(100) > 98) {
                    srcRect = new Rectangle(new Point(1 * 16, 7 * 16), new Size(16, 16));
                    graphics.DrawImage(Icons, destRect, srcRect, GraphicsUnit.Pixel);
                }
            }
            var semiTransBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
            var rect = new Rectangle(0, 0, 1500, 900);
            graphics.FillRectangle(semiTransBrush, rect);
        }
        private void DrawBestScores(Graphics graphics, GameEngine gameEngine) {
            _results ??= gameEngine.DataLayer.GetBestScores();
            var counter = 1;
            var keyValuePairs = _results.Reverse();
            foreach (var result in keyValuePairs) {
                graphics.DrawString($"{counter}. {result.Value}: {result.Key}",
                    MainFont, WhiteBrush, 520, 180 + 30 * counter);
                counter++;
                if (counter == 18) break;
            }
        }
        private void DrawSettings(Graphics graphics, GameEngine gameEngine) {
            var selected = new Rectangle(520, 210 + gameEngine.CurrentSubAction * 40, 940, 40);
            graphics.FillRectangle(DarkBrush,
                selected);
            graphics.DrawString($"Difficulty: {gameEngine.DataLayer.Settings.Difficulty}",
                MainFont, WhiteBrush, 520, 210);
            graphics.DrawString($"Size X: {gameEngine.DataLayer.Settings.SizeX}",
                MainFont, WhiteBrush, 520, 250);
            graphics.DrawString($"Size Y: {gameEngine.DataLayer.Settings.SizeY}",
                MainFont, WhiteBrush, 520, 290);
            graphics.DrawString($"Fps: {gameEngine.DataLayer.Settings.Fps}",
                MainFont, WhiteBrush, 520, 330);
            graphics.DrawString($"Tick Rate: {gameEngine.DataLayer.Settings.TickRate}",
                MainFont, WhiteBrush, 520, 370);
            graphics.DrawString("Use A and S to change parameters, press ENTER to save",
                MainFont, WhiteBrush, 520, 700);
        }
    }
}