using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ActionBuilderMVVM.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ActionBuilderMVVM.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ActionModel
    {
        public string Path { get; set; }

        public enum FrameType
        {
            Startup,
            Active,
            Recovery,
            Buffer
        }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        private List<FrameType> _frames;

        [JsonProperty]
        private Vector2 _infinite;

        public ActionModel()
        {
            _frames = new List<FrameType>();
            _infinite = new Vector2(-1, -1);
            Anchor = new Vector2(0, 0);
            Hurtboxes = new List<List<Box>>();
        }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
        public ActionType Type { get; set; }

        [JsonProperty]
        public Vector2 Anchor { get; set; }

        [JsonProperty]
        public List<List<Box>> Hitboxes { get; set; }

        [JsonProperty]
        public List<List<Box>> Hurtboxes { get; set; }

        [JsonProperty]
        public List<List<Box>> Grabboxes { get; set; }

        [JsonProperty]
        public List<List<Box>> Armorboxes { get; set; }

        [JsonProperty]
        public List<List<Box>> Collisionboxes { get; set; }

        [JsonProperty]
        public List<List<Box>> Databoxes { get; set; }

        [JsonProperty]
        public List<FrameProperty> FrameProperties { get; set; }

        public List<List<Box>> AllBoxes => CollectionUtils
            .Concat(Hitboxes, Hurtboxes, Grabboxes, Armorboxes, Collisionboxes, Databoxes).ToList();

        public int FrameCount => _frames.Count;

        public float InfiniteRangeMin
        {
            get => _infinite.X;
            set => _infinite.X = value;
        }

        public float InfiniteRangeMax
        {
            get => _infinite.Y;
            set => _infinite.Y = value;
        }

        public FrameType FrameTypeAt(int i) => _frames[i];

        [JsonObject(MemberSerialization.OptIn)]
        public class VelocityModifier
        {
            public enum ModificationType
            {
                Target,
                IgnoreX,
                IgnoreY,
                IgnoreBoth
            }

            public VelocityModifier(Vector2 velocity = default,
                ModificationType modificationType = ModificationType.IgnoreBoth)
            {
                Velocity = velocity;
                Modification = modificationType;
            }

            [JsonProperty]
            public Vector2 Velocity { get; set; }

            [JsonConverter(typeof(StringEnumConverter)), JsonProperty]
            public ModificationType Modification { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class FrameProperty
        {
            public FrameProperty(VelocityModifier velocity) => DetailedVelocity = velocity;

            public FrameProperty() => DetailedVelocity = new VelocityModifier();

            [JsonProperty]
            public VelocityModifier DetailedVelocity { get; set; }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class Box
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

        public Box(BoxType type, int x, int y, double width, double height, double damage, double baseKnockback, double knockbackGrowth,
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

        public Box() : this(BoxType.Hit, 0, 0, 5, 5, 0, 0, 0, 0, 1) { }

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
