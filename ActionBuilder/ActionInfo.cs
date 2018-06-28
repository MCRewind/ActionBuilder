using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.Serialization;

namespace ActionBuilder
{
    [DataContract]
    internal class ActionInfo
    {
        enum FrameType { Startup, Active, Recovery, Buffer }

        [DataMember]
        public string Name;

        [DataMember]
        private List<FrameType> _frames; 

        public int FrameCount { get => _frames.Count; }

        [DataMember]
        public Types.ActionType Type;

        [DataMember]
        private Vector2 _infinite;
        public float InfiniteRangeMin { get => _infinite.X; set => _infinite.X = value; }
        public float InfiniteRangeMax { get => _infinite.Y; set => _infinite.Y = value; }

        public ActionInfo()
        {
            _frames = new List<FrameType> { FrameType.Startup };
            _infinite = new Vector2(-1, -1);
            Hitboxes = new List<List<Box>>();
            Hurtboxes = new List<List<Box>>();
        }

        [DataContract]
        public class Box
        {
            [DataMember]
            public Vector2 KnockbackAngle;
            [DataMember]
            public double Damage, KnockbackStrength;
            [DataMember]
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
        }

        [DataMember]
        public List<List<Box>> Hitboxes, Hurtboxes;

        public void InsertFrame(int index)
        {
            if (_frames.Count <= 0)
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
            if (_frames.Count <= 0) return;

            if (index > _frames.Count)
                _frames.RemoveAt(_frames.Count - 1);
            else if (index > 0)
                _frames.RemoveAt(index - 1);
            else
                _frames.RemoveAt(0);
        }

    }

}
