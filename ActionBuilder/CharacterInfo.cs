﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ActionBuilder
{
    [DataContract]
    internal class CharacterInfo
    {
        [DataMember]
        public string Name { get; set; }


    }
}
