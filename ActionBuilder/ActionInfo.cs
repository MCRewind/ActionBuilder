using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.Serialization;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ActionBuilder
{
    public class ActionInfo : IEquatable<ActionInfo>
    {
        enum FrameType { Startup, Active, Recovery, Buffer }

        public string Name;

        [JsonProperty (ItemConverterType = typeof(StringEnumConverter))]
        private List<FrameType> _frames; 

        [JsonIgnore]
        public int FrameCount => _frames.Count;

        [JsonConverter(typeof(StringEnumConverter))]
        public Types.ActionType Type;

        [JsonProperty]
        private Vector2 _infinite;

        [JsonIgnore]
        public float InfiniteRangeMin { get => _infinite.X; set => _infinite.X = value; }
        [JsonIgnore]
        public float InfiniteRangeMax { get => _infinite.Y; set => _infinite.Y = value; }
        
        [JsonConstructor]
        public ActionInfo(bool b)
        {
            _infinite = new Vector2(-1, -1);
        }

        public ActionInfo()
        {
            _frames = new List<FrameType> { FrameType.Startup };
            _infinite = new Vector2(-1, -1);
        }

        public class Box : IEquatable<Box>
        {
           public Vector2 KnockbackAngle;
            public double Damage, KnockbackStrength;
            public int Lifespan, X, Y, Width, Height;

            public Box(int x, int y, int width, int height, double damage, double knockbackStrength, Vector2 knockbackAngle, int lifespan)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Damage = damage;
                KnockbackStrength = knockbackStrength;
                KnockbackAngle = knockbackAngle;
                Lifespan = lifespan;
            }

            public Box() : this(0, 0, 5, 5, 0, 0, new Vector2(), 1) { }

            public void SetPos(int x, int y) { this.X = x; this.Y = y; }
            public void SetDims(int w, int h) { this.Width = w; this.Height = h; }

            public bool Equals(Box other)
            {
                if (other is null) return false;
                if (ReferenceEquals(this, other)) return true;
                return KnockbackAngle.Equals(other.KnockbackAngle) 
                       && Damage.Equals(other.Damage) 
                       && KnockbackStrength.Equals(other.KnockbackStrength) 
                       && Lifespan == other.Lifespan 
                       && X == other.X && Y == other.Y 
                       && Width == other.Width && Height == other.Height;
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals(obj as Box);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = KnockbackAngle.GetHashCode();
                    hashCode = (hashCode * 397) ^ Damage.GetHashCode();
                    hashCode = (hashCode * 397) ^ KnockbackStrength.GetHashCode();
                    hashCode = (hashCode * 397) ^ Lifespan;
                    hashCode = (hashCode * 397) ^ X;
                    hashCode = (hashCode * 397) ^ Y;
                    hashCode = (hashCode * 397) ^ Width;
                    hashCode = (hashCode * 397) ^ Height;
                    return hashCode;
                }
            }

            public static bool operator ==(Box left, Box right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Box left, Box right)
            {
                return !Equals(left, right);
            }
        }

        public List<List<Box>> Hitboxes, Hurtboxes;

        public void InsertFrame(int index)
        {
            if (_frames.Count == 0)
                _frames.Add(FrameType.Startup);
            else if (index > _frames.Count)
                _frames.Add(_frames[_frames.Count - 1]);
            else if (index > 0)
                _frames.Insert(index, _frames[index - 1]);
            else
                _frames.Prepend(FrameType.Startup);
        }

        public void RemoveFrame(int index)
        {
            if (_frames.Count == 0) return;

            if (index > _frames.Count)
                _frames.RemoveAt(_frames.Count - 1);
            else if (index > 0)
                _frames.RemoveAt(index - 1);
            else
                _frames.RemoveAt(0);
        }

        public bool Equals(ActionInfo other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) 
                   && Equals(_frames, other._frames) 
                   && Type == other.Type 
                   && _infinite.Equals(other._infinite)
                   && Hitboxes.SequenceEqual(other.Hitboxes)
                   && Hurtboxes.SequenceEqual(other.Hurtboxes);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals(obj as ActionInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_frames != null ? _frames.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Type;
                hashCode = (hashCode * 397) ^ _infinite.GetHashCode();
                hashCode = (hashCode * 397) ^ (Hitboxes != null ? Hitboxes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Hurtboxes != null ? Hurtboxes.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ActionInfo left, ActionInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ActionInfo left, ActionInfo right)
        {
            return !Equals(left, right);
        }
    }

}
