﻿using System;
using System.Windows.Forms;
using ClassLibrary;
using ClassLibrary.Entities;
using ClassLibrary.Entities.Player;
using ClassLibrary.InputProcessors;
using ClassLibrary.SoundPlayer;

namespace BoulderDashForms.InputProcessors {
    public class GameInputProcessor : InputProcessor {
        public void ProcessKeyDown(Keys key, Func<Player> getPlayer,
            Action<float> changeVolume
        ) {
            var player = getPlayer();
            switch (key) {
                case Keys.W:
                    player.Move(MoveDirectionEnum.Vertical, -1);
                    player.Keyboard.W = KeyboardEnum.Enabled;
                    break;
                case Keys.S:
                    player.Move(MoveDirectionEnum.Vertical, 1);
                    player.Keyboard.S = KeyboardEnum.Enabled;
                    break;
                case Keys.A:
                    player.Move(MoveDirectionEnum.Horizontal, -1);
                    player.PlayerAnimator.Reverse = -1;
                    player.Keyboard.A = KeyboardEnum.Enabled;
                    break;
                case Keys.D:
                    player.Move(MoveDirectionEnum.Horizontal, 1);
                    player.PlayerAnimator.Reverse = 1;
                    player.Keyboard.D = KeyboardEnum.Enabled;
                    break;
                case Keys.T:
                    player.Teleport();
                    player.Keyboard.T = KeyboardEnum.Enabled;
                    break;
                case Keys.Space:
                    player.UseEnergyConverter();
                    player.Keyboard.Space = KeyboardEnum.Enabled;
                    break;
                case Keys.Q:
                    player.ConvertNearStonesInDiamonds();
                    player.Keyboard.Q = KeyboardEnum.Enabled;
                    break;
                case Keys.E:
                    player.UseDynamite();
                    player.Keyboard.E = KeyboardEnum.Enabled;
                    break;
                case Keys.R:
                    player.Attack();
                    player.Keyboard.R = KeyboardEnum.Enabled;
                    break;
                case Keys.Add:
                    changeVolume(0.1f);
                    break;
                case Keys.Subtract:
                    changeVolume(-0.1f);
                    break;
                case Keys.Escape:
                    break;
            }
        }

        public void ProcessKeyUp(Keys key, Func<Player> getPlayer, Action<GameStatusEnum> changeGameStatus) {
            var player = getPlayer();
            switch (key) {
                case Keys.W:
                    player.SetAnimation(0);
                    player.Keyboard.W = KeyboardEnum.Disabled;
                    break;
                case Keys.S:
                    player.SetAnimation(0);
                    player.Keyboard.S = KeyboardEnum.Disabled;
                    break;
                case Keys.A:
                    player.SetAnimation(0);
                    player.Keyboard.A = KeyboardEnum.Disabled;
                    break;
                case Keys.D:
                    player.SetAnimation(0);
                    player.Keyboard.D = KeyboardEnum.Disabled;
                    break;
                case Keys.T:
                    player.Keyboard.T = KeyboardEnum.Disabled;
                    break;
                case Keys.Space:
                    player.Keyboard.Space = KeyboardEnum.Disabled;
                    player.CheckAdrenalineCombo();
                    break;
                case Keys.Q:
                    player.Keyboard.Q = KeyboardEnum.Disabled;
                    break;
                case Keys.E:
                    player.Keyboard.E = KeyboardEnum.Disabled;
                    break;
                case Keys.R:
                    player.Keyboard.R = KeyboardEnum.Disabled;
                    break;
                case Keys.Escape:
                    changeGameStatus(GameStatusEnum.Menu);
                    break;
            }
        }
    }
}