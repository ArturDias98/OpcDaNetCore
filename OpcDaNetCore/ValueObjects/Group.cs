using System;
using System.Collections.Generic;
using System.Linq;

namespace OpcDaNetCore.ValueObjects
{
    public class Group
    {
        public Group(string name, int updateRate, IEnumerable<string> items)
        {
            ValidateName(name);
            ValidateItems(items);
            ValidateUpdateRate(updateRate);

            Name = name;
            UpdateRate = updateRate;
            Items = items;
        }

        public Group(string name, int updateRate)
        {
            ValidateName(name);
            ValidateUpdateRate(updateRate);

            Name = name;
            UpdateRate = updateRate;
        }

        public Group(string name)
        {
            ValidateName(name);

            Name = name;
        }

        public Group(string name, IEnumerable<string> items)
        {
            ValidateName(name);
            ValidateItems(items);

            Name = name;
            Items = items;
        }

        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The group name is required");
            }
        }

        private void ValidateItems(IEnumerable<string> items)
        {
            if (items?.Any() != true)
            {
                throw new ArgumentException("Group without items");
            }
        }

        private void ValidateUpdateRate(int updateRate)
        {
            if (updateRate <= 0)
            {
                throw new ArgumentException("Invalid update rate");
            }
        }

        public string Name { get; private set; }
        public int UpdateRate { get; private set; } = 1000;
        public IEnumerable<string> Items { get; } = Enumerable.Empty<string>();

        internal void Default()
        {
            Name = "Group 1";
            UpdateRate = 1000;
        }
    }
}