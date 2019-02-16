using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ActionBuilderMVVM.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BoxModel
    {
        public enum BoxType
        {
            Hit,
            Hurt,
            Grab,
            Armor,
            Collision,
            Data,
            Null
        }

        public BoxModel(BoxType type, int x, int y, double width, double height, double damage, double baseKnockback, double knockbackGrowth,
            double knockbackAngle, int lifespan)
        {
            Type = type;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Damage = damage;
            BaseKnockback = baseKnockback;
            KnockbackGrowth = knockbackGrowth;
            KnockbackAngle = knockbackAngle;
            Lifespan = lifespan;
        }

        public BoxModel() : this(BoxType.Hit, 0, 0, 5, 5, 0, 0, 0, 0, 1) { }

        [JsonProperty]
        public double Damage { get; set; }

        [JsonProperty]
        public double BaseKnockback { get; set; }

        [JsonProperty]
        public double KnockbackGrowth { get; set; }

        [JsonProperty]
        public double KnockbackAngle { get; set; }

        [JsonProperty]
        public int Lifespan { get; set; }

        [JsonProperty]
        public int X { get; set; }

        [JsonProperty]
        public int Y { get; set; }

        [JsonProperty]
        public double Width { get; set; }

        [JsonProperty]
        public double Height { get; set; }

        [JsonConverter(typeof(StringEnumConverter)), JsonProperty]

        public BoxType Type { get; set; }
    }
}
