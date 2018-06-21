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

        public class Box
        {
            public Vector2 KnockbackAngle;
            public float X, Y, Width, Height, Damage, KnockbackStrength;
            public int Lifespan;

            public Box(float x, float y, float width, float height, float damage, float knockbackStrength, Vector2 knockbackAngle, int lifespan)
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

            public void SetPos(double x, double y) { this.X = (float) x; this.Y = (float) y; }
            public void SetDims(double w, double h) { this.Width = (float) w; this.Height = (float) h; }
            public void SetDamage(float dmg) { this.Damage = dmg; }
            public void SetKnockbackStrength(float strength) { this.KnockbackStrength = strength; }
            public void SetKnockbackAngle(Vector2 angle) { this.KnockbackAngle = angle; }
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
