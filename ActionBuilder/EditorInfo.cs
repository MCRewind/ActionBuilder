using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ActionBuilder
{
    [DataContract]
    class EditorInfo
    {

        [DataMember]
        public String texturePath { get; set; }

    }
}
