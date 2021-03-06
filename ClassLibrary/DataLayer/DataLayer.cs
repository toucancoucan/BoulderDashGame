﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;

namespace ClassLibrary.DataLayer {
    public class DataLayer {
        private readonly string _customLevelsDatabase =
            Path.Combine(Environment.CurrentDirectory, @"gameFiles\", "CustomLevels.db");

        private readonly JsonController _jsonController = new JsonController();
        private readonly string _savesDatabase = Path.Combine(Environment.CurrentDirectory, @"gameFiles\", "Saves.db");

        private readonly string _scoresDatabase =
            Path.Combine(Environment.CurrentDirectory, @"gameFiles\", "BestScores.db");

        public List<CustomLevel> Levels;
        public Settings Settings;

        public DataLayer() {
            GetSettings();
        }
        private async void GetSettings() {
            Settings = await _jsonController.GetSettings();
        }

        public async void SaveSettings() {
            await _jsonController.WriteSettings(Settings);
        }

        public List<Save> GetAllGameSaves() {
            using var db = new LiteDatabase(_savesDatabase);
            var col = db.GetCollection<Save>("saves");
            var searchResult = col.FindAll();
            return searchResult.ToList();
        }

        public void AddGameSave(Save save) {
            using var db = new LiteDatabase(_savesDatabase);
            var col = db.GetCollection<Save>("saves");
            col.EnsureIndex(x => x.Name);
            if (col.Exists(x => x.Name == save.Name)) col.Update(save);
            else col.Insert(save);
        }

        public async void AddCustomLevel(List<CustomLevel> levels) {
            await _jsonController.AddCustomLevel(levels);
        }

        public async void GetAllCustomLevels() {
            Levels = await _jsonController.GetCustomLevels();
        }

        public void DeleteGameSave(Save save) {
            using var db = new LiteDatabase(_savesDatabase);
            var col = db.GetCollection<Save>("saves");
            col.Delete(save.Id);
        }

        public void ChangeGameSave(Save save) {
            using var db = new LiteDatabase(_savesDatabase);
            var col = db.GetCollection<Save>("saves");
            col.Update(save);
        }

        public void AddBestScore(string name, int score) {
            using var db = new LiteDatabase(_scoresDatabase);
            var col = db.GetCollection<Score>("scores");
            col.EnsureIndex(x => x.Name);
            var currentScore = new Score {Name = name, Value = score};
            if (col.Exists(x => x.Name == name))
                col.Update(currentScore);
            else
                col.Insert(currentScore);
        }

        public SortedDictionary<int, string> GetBestScores() {
            var result = new SortedDictionary<int, string>();
            using var db = new LiteDatabase(_scoresDatabase);
            var col = db.GetCollection<Score>("scores");
            var all = col.FindAll();
            foreach (var save in all)
                if (!result.ContainsKey(save.Value))
                    result.Add(save.Value, save.Name);
            return result;
        }
    }
}