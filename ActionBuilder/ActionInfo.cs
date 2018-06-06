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
        private bool infinite;

        public ActionInfo()
        {
            frames = new List<FrameType>();
            frames.Add(FrameType.Startup);
            infinite = false;
        }

        private struct Box
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
