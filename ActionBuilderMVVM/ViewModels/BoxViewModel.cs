using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionBuilderMVVM.Models;

namespace ActionBuilderMVVM.ViewModels
{
    class BoxViewModel
    {
        public float X => Box.X;
        public float Y => Box.Y;
        public double Width => Box.Width * _scale;
        public double Height => Box.Height * _scale;
        public BoxModel.BoxType Type => Box.Type;

        private float _scale = 100;

        public BoxModel Box { get; set; }

    }
}
