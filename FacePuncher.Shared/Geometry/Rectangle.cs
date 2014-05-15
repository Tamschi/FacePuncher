﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FacePuncher.Geometry
{
    /// <summary>
    /// Structure representing a rectangle with defined location
    /// and dimensions using Cartesian coordinates.
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// A rectangle with zero size at the origin.
        /// </summary>
        public static readonly Rectangle Zero = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// Horizontal position of the left edge of the rectangle.
        /// </summary>
        public int Left;

        /// <summary>
        /// Vertical position of the top edge of the rectangle.
        /// </summary>
        public int Top;

        /// <summary>
        /// Horizontal size of the rectangle.
        /// </summary>
        public int Width;

        /// <summary>
        /// Vertical size of the rectangle.
        /// </summary>
        public int Height;

        /// <summary>
        /// Horizontal position of the right edge of the rectangle.
        /// </summary>
        public int Right
        {
            get { return Left + Width; }
            set { Width = value - Left; }
        }

        /// <summary>
        /// Vertical position of the bottom edge of the rectangle.
        /// </summary>
        public int Bottom
        {
            get { return Top + Height; }
            set { Height = value - Top; }
        }

        /// <summary>
        /// Position of the top left vertex of the rectangle.
        /// </summary>
        public Position TopLeft
        {
            get { return new Position(Left, Top); }
            set { Left = value.X; Top = value.Y; }
        }

        /// <summary>
        /// Position of the top right vertex of the rectangle.
        /// </summary>
        public Position TopRight
        {
            get { return new Position(Right, Top); }
            set { Right = value.X; Top = value.Y; }
        }

        /// <summary>
        /// Position of the bottom left vertex of the rectangle.
        /// </summary>
        public Position BottomLeft
        {
            get { return new Position(Left, Bottom); }
            set { Left = value.X; Bottom = value.Y; }
        }

        /// <summary>
        /// Position of the bottom right vertex of the rectangle.
        /// </summary>
        public Position BottomRight
        {
            get { return new Position(Right, Bottom); }
            set { Right = value.X; Bottom = value.Y; }
        }

        /// <summary>
        /// Initializes a new rectangle structure based on the
        /// positions of its upper left and lower right vertices.
        /// </summary>
        /// <param name="tl">Position of the top left vertex of the rectangle.</param>
        /// <param name="br">Position of the bottom right vertex of the rectangle.</param>
        public Rectangle(Position tl, Position br)
        {
            Left = tl.X; Top = tl.Y;

            Width = br.X - tl.X;
            Height = br.Y - tl.Y;
        }

        /// <summary>
        /// Initializes a new rectangle structure based on the X and Y
        /// coordinates of its top left vertex and its width and height.
        /// </summary>
        /// <param name="x">Horizontal position of the left edge.</param>
        /// <param name="y">Vertical position of the top edge.</param>
        /// <param name="w">Horizontal size of the rectangle.</param>
        /// <param name="h">Vertical size of the rectangle.</param>
        public Rectangle(int x, int y, int w, int h)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = h;
        }

        /// <summary>
        /// Tests to see if the given position is within the bounds
        /// of this rectangle.
        /// </summary>
        /// <param name="pos">Position to perform an intersection
        /// test with.</param>
        /// <returns>True if the position is within the bounds of this
        /// rectangle, and false otherwise.</returns>
        public bool Intersects(Position pos)
        {
            return this.Left <= pos.X && this.Top <= pos.Y
                && this.Right > pos.X && this.Bottom > pos.Y;
        }

        /// <summary>
        /// Tests to see if the given rectangle intersects with this one.
        /// </summary>
        /// <param name="rect">Rectangle to perform an intersection
        /// test with.</param>
        /// <returns>True if the rectangle has a non-zero intersection
        /// area with this one, and false otherwise.</returns>
        public bool Intersects(Rectangle rect)
        {
            return this.Right > rect.Left && this.Bottom > rect.Top
                && rect.Right > this.Left && rect.Bottom > this.Top;
        }

        /// <summary>
        /// Finds the rectangle describing the intersection between
        /// this and another rectangle if one exists, and otherwise
        /// an empty rectangle.
        /// </summary>
        /// <param name="rect">Rectangle to find the intersection with.</param>
        /// <returns>The intersecting rectangle if one exists, and
        /// otherwise the zero rectangle.</returns>
        public Rectangle Intersection(Rectangle rect)
        {
            if (!Intersects(rect)) return Rectangle.Zero;

            var tl = new Position(
                Math.Max(this.Left, rect.Left),
                Math.Max(this.Top, rect.Top));

            var br = new Position(
                Math.Min(this.Right, rect.Right),
                Math.Min(this.Bottom, rect.Bottom));

            return new Rectangle(tl, br);
        }

        /// <summary>
        /// Finds the nearest position within the rectangle to
        /// the specified location.
        /// </summary>
        /// <param name="pos">Position to find the closest position to.</param>
        /// <returns>The closest position within the rectangle to the
        /// specified location.</returns>
        public Position NearestPosition(Position pos)
        {
            return new Position(pos.X.Clamp(Left, Right - 1), pos.Y.Clamp(Top, Bottom - 1));
        }

        /// <summary>
        /// Gets a set of all positions within the rectangle.
        /// </summary>
        public IEnumerable<Position> Positions
        {
            get
            {
                var rect = this;

                return
                    Enumerable.Range(Top, Height).SelectMany(y =>
                        Enumerable.Range(rect.Left, rect.Width)
                            .Select(x => new Position(x, y)));
            }
        }

        #region Operator Overloads
        public static Rectangle operator +(Rectangle rect, Position offset)
        {
            return new Rectangle(rect.TopLeft + offset, rect.BottomRight + offset);
        }

        public static Rectangle operator -(Rectangle rect, Position offset)
        {
            return new Rectangle(rect.TopLeft - offset, rect.BottomRight - offset);
        }

        public static bool operator ==(Rectangle a, Rectangle b)
        {
            return a.Left == b.Left && a.Top == b.Top && a.Width == b.Width && a.Height == b.Height;
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return a.Left != b.Left || a.Top != b.Top || a.Width != b.Width || a.Height != b.Height;
        }
        #endregion

        /// <summary>
        /// Indicates whether this instance and another specified
        /// object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>True if this instance is equal to the given object,
        /// and false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is Rectangle && this == ((Rectangle) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return Top ^ Left ^ Width ^ Height;
        }

        /// <summary>
        /// Returns a string representing this rectangle.
        /// </summary>
        /// <returns>A string representing this rectangle.</returns>
        public override string ToString()
        {
            return String.Format("({0} {1} {2} {3})", Left, Top, Width, Height);
        }
    }
}
