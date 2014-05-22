using FacePuncher.Geometry;
using FacePuncher.Graphics;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacePuncher.Network
{
    [ProtoContract]
    public class VisibleLevelState
    {
        [ProtoMember(1)]
        public Rectangle Rect { get; private set; }
        [ProtoMember(2)]
        private TileAppearance[][] _appearances;
        public TileAppearance this[int relX, int relY]
        {
            get { return _appearances[relX][relY]; }
        }
    }
}
