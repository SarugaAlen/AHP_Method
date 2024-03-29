﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHP_Method.Model
{
    internal class Alternativa : ICloneable
    {
        public string Name { get; set; }
        public double Koristnost { get; set; }

        public Alternativa()
        {
            
        }

        public Alternativa(string name)
        {
            Name = name;
        }

        public Alternativa(string name, double koristnost)
        {
            Name = name;
            Koristnost = koristnost;
        }

        public object Clone()
        {
            return new Alternativa
            {
                Name = this.Name,
                Koristnost = this.Koristnost
            };
        }

    }
}
