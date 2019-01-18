using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionBuilderMVVM.Models
{
    class BoxRectModel
    {

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        private float _scale = 10;

        public BoxRectModel(float gameX, float gameY, float gameWidth, float gameHeight)
        {
            X = gameX * _scale;
            Y = gameY * _scale;
            Width = gameWidth * _scale;
            Height = gameHeight * _scale;
        }

    }
}
