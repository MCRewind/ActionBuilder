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
    namespace ActionBuilderMVVM
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class ActionModel
        {
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
            public ActionType Type { get; private set; }

            [JsonProperty]
            public Vector2 Anchor { get; private set; }

            [JsonProperty]
            public List<List<Box>> Hitboxes { get; private set; }

            [JsonProperty]
            public List<List<Box>> Hurtboxes { get; private set; }

            [JsonProperty]
            public List<List<Box>> Grabboxes { get; private set; }

            [JsonProperty]
            public List<List<Box>> Armorboxes { get; private set; }

            [JsonProperty]
            public List<List<Box>> Collisionboxes { get; private set; }

            [JsonProperty]
            public List<List<Box>> Databoxes { get; private set; }

            [JsonProperty]
            public List<FrameProperty> FrameProperties { get; private set; }

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

                public Box(BoxType type, int x, int y, double width, double height, double damage, double knockbackStrength,
                    double knockbackAngle, int lifespan)
                {
                    Type = type;
                    X = x;
                    Y = y;
                    Width = width;
                    Height = height;
                    Damage = damage;
                    KnockbackStrength = knockbackStrength;
                    KnockbackAngle = knockbackAngle;
                    Lifespan = lifespan;
                }

                public Box() : this(BoxType.Hit, 0, 0, 5, 5, 0, 0, 0, 1) { }

                [JsonProperty]
                public double Damage { get; private set; }

                [JsonProperty]
                public double KnockbackStrength { get; private set; }

                [JsonProperty]
                public double KnockbackAngle { get; private set; }

                [JsonProperty]
                public int Lifespan { get; private set; }

                [JsonProperty]
                public int X { get; private set; }

                [JsonProperty]
                public int Y { get; private set; }

                [JsonProperty]
                public double Width { get; private set; }

                [JsonProperty]
                public double Height { get; private set; }

                [JsonConverter(typeof(StringEnumConverter)), JsonProperty]

                public BoxType Type { get; private set; }
            }

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
                public Vector2 Velocity { get; private set; }

                [JsonConverter(typeof(StringEnumConverter)), JsonProperty]
                public ModificationType Modification { get; private set; }
            }

            [JsonObject(MemberSerialization.OptIn)]
            public class FrameProperty
            {
                public FrameProperty(VelocityModifier velocity) => DetailedVelocity = velocity;

                public FrameProperty() => DetailedVelocity = new VelocityModifier();

                [JsonProperty]
                public VelocityModifier DetailedVelocity { get; private set; }
            }
        }
    }
}
