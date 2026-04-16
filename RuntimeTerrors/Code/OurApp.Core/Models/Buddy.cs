using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Models
{
    public class Buddy
    {   
        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Introduction {  get; private set; }

        public Buddy(int id, string name, string introduction) {
            Id = id;
            Name = name;
            Introduction = introduction;
        }

    }
}
