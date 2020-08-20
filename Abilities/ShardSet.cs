﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Abilities
{
    public class ShardSet
    {
        private HashSet<int> collected = new HashSet<int>();

        public bool Has(int id) => collected.Contains(id);
        public void Add(int id) => collected.Add(id);

        public int Count => collected.Count;

        public List<int> ToList() => collected.ToList();
    }
}