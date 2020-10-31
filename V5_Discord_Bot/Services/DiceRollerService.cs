﻿using System;
using System.Collections.Generic;
using V5_Discord_Bot.Model;

namespace V5_Discord_Bot.Services
{
    public class DiceRollerService
    {
        private Random _d10;
        private Dictionary<ulong, RollResult> _rollCache;
        private readonly HungerService _hungerService;

        public DiceRollerService(HungerService hungerService)
        {
            _d10 = new Random();
            _rollCache = new Dictionary<ulong , RollResult>();
            _hungerService = hungerService;
        }

        public RollResult Roll(ulong userId, int amount)
        {
            var hunger = _hungerService.GetHunger(userId);
            var result = new RollResult
            {
                HungerDice = RollDice(hunger),
                NormalDice = RollDice(amount - hunger),
            };
            SetPreviousRoll(userId, result);
            return result;
        }

        public (bool gainedHunger, int? currentHunger, bool hungerFrenzy) Rouse(ulong userId)
        {
            var result = RollDie();
            if (result >= 6)
                return (true, null, false);

            var (hunger, hungerFrenzy) = _hungerService.IncrementHunger(userId);
            return (false, hunger, hungerFrenzy);
        }

        private ICollection<int> RollDice(int amount)
        {
            var rolls = new List<int>();
            for (var i = 0; i < amount; i++)
            {
                rolls.Add(RollDie());
            }
            rolls.Sort();
            return rolls;
        }

        private int RollDie()
        {
            return _d10.Next(1, 11);
        }

        private void SetPreviousRoll(ulong userId, RollResult rollResult)
        {
            _rollCache[userId] = rollResult;
        }

        private RollResult? GetPreviousRoll(ulong userId)
        {
            if (_rollCache.TryGetValue(userId, out var rollResult))
                return rollResult;
            return null;
        }
    }
}