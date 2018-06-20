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
        public string name;

        [DataMember]
        private List<FrameType> frames; 

        public int FrameCount { get => frames.Count; }

        [DataMember]
        public Types.ActionType type;

        [DataMember]
        private Vector2 infinite;
        public float InfiniteRangeMin { get => infinite.X; set => infinite.X = value; }
        public float InfiniteRangeMax { get => infinite.Y; set => infinite.Y = value; }

        public ActionInfo()
        {
            frames = new List<FrameType>();
            frames.Add(FrameType.Startup);
            infinite = new Vector2(-1, -1);
        }

        public class Box
        {
            public Vector2 knockbackAngle;
            public float x, y, width, height, damage, knockbackStrength;

            public Box(float x, float y, float width, float height, float damage, float knockbackStrength, Vector2 knockbackAngle)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
                this.damage = damage;
                this.knockbackStrength = knockbackStrength;
                this.knockbackAngle = knockbackAngle;
            }

            public Box(Boolean baseBox) : this(0, 0, 5, 5, 0, 0, new Vector2()) { }

            public void setPos(double x, double y) { this.x = (float) x; this.y = (float) y; }
            public void setDims(double w, double h) { this.width = (float) w; this.height = (float) h; }
            public void setDamage(float dmg) { this.damage = dmg; }
            public void setKnockbackStrength(float strength) { this.knockbackStrength = strength; }
            public void setKnockbackAngle(Vector2 angle) { this.knockbackAngle = angle; }
        }

        [DataMember]
        private List<Box> hitboxes, hurtboxes;

        public void insertFrame(int index)
        {
            if (frames.Count > 0)
                if (index > frames.Count)
                    frames.Add(frames[frames.Count - 1]);
                else if (index > 0)
                    frames.Insert(index, frames[index - 1]);
                else
                    frames.Prepend(FrameType.Startup);
            else
                frames.Add(FrameType.Startup);
        }

        public void removeFrame(int index)
        {
            if (frames.Count > 0)
                if (index > frames.Count)
                    frames.RemoveAt(frames.Count - 1);
                else if (index > 0)
                    frames.RemoveAt(index - 1);
                else
                    frames.RemoveAt(0);
        }

    }

}
