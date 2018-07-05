using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ActionBuilder
{
    public class ActionInfo
    {
        public enum FrameType
        {
            [UsedImplicitly] Startup,
            [UsedImplicitly] Active,
            [UsedImplicitly] Recovery,
            [UsedImplicitly] Buffer
        }

        public string Name;

        [JsonProperty (ItemConverterType = typeof(StringEnumConverter))]
        private List<FrameType> _frames; 

        [JsonIgnore]
        public int FrameCount => _frames.Count;

        public FrameType FrameTypeAt(int i) => _frames[i];

        [JsonConverter(typeof(StringEnumConverter))]
        public Types.ActionType Type;

        [JsonProperty]
        private Vector2 _infinite;

        [JsonIgnore]
        public float InfiniteRangeMin { get => _infinite.X; set => _infinite.X = value; }
        [JsonIgnore]
        public float InfiniteRangeMax { get => _infinite.Y; set => _infinite.Y = value; }

        public Vector2 Anchor;

        public ActionInfo()
        {
            _frames = new List<FrameType> { FrameType.Startup };
            _infinite = new Vector2(-1, -1);
            Anchor = new Vector2(0, 0);
        }

        public class Box
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

            public void SetPos(int x, int y) { X = x; Y = y; }
            public void SetDims(int w, int h) { Width = w; Height = h; }
        }

        public List<List<Box>> Hitboxes, Hurtboxes, Grabboxes, Armorboxes, Collisionboxes, Databoxes;

        private ref List<List<Box>> CurrentBoxList(int type)
        {
            switch (type)
            {
                case 0:
                    return ref Hitboxes;
                case 1:
                    return ref Hurtboxes;
                case 2:
                    return ref Grabboxes;
                case 3:
                    return ref Armorboxes;
                case 4:
                    return ref Collisionboxes;
                case 5:
                    return ref Databoxes;
                default:
                    return ref Hitboxes;
            }
        }

        public void SetFrameType(int index, FrameType type)
        {
            if (_frames.Count == 0) return;
            if (index > _frames.Count - 1) return;
            if (index < 0) return;

            _frames[index] = type;
        }

        // type: 0 - hit, 1 - hurt
        public void InsertBoxList(int index, int type)
        {
            var list = CurrentBoxList(type);
            if (list.Count == 0)
                list.Add(new List<Box>());
            else if (index > _frames.Count)
                list.Add(list[list.Count - 1]);
            else if (index > 0)
                list.Insert(index, list[index - 1]);
            else
                list.Add(new List<Box>());
        }

        public void RemoveBoxList(int index, int type)
        {
            var list = CurrentBoxList(type);
            if (list.Count == 0) return;

            if (index > list.Count)
                list.RemoveAt(list.Count - 1);
            else if (index > 0)
                list.RemoveAt(index - 1);
            else
                list.RemoveAt(0);
        }

        public void InsertFrame(int index)
        {
            if (_frames.Count == 0)
                _frames.Add(FrameType.Startup);
            else if (index > _frames.Count)
                _frames.Add(_frames[_frames.Count - 1]);
            else if (index > 0)
                _frames.Insert(index, _frames[index - 1]);
            else
                _frames.Add(FrameType.Startup);
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
    }

}
