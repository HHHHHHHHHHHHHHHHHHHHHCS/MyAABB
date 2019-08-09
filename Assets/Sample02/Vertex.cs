using UnityEngine;

namespace QHull
{
    using System;

/**
 * Represents vertices of the hull, as well as the points from
 * which it is formed.
 *
 * @author John E. Lloyd, Fall 2004
 */
    public class Vertex
    {
        /**
         * Spatial point associated with this vertex.
         */
        public Vector3 pnt;

        /**
         * Back index into an array.
         */
        public int index;

        /**
         * List forward link.
         */
        public Vertex prev;

        /**
         * List backward link.
         */
        public Vertex next;

        /**
         * Current face that this vertex is outside of.
         */
        public Face face;

        /**
         * Constructs a vertex and sets its coordinates to 0.
         */
        public Vertex()
        {
            pnt = new Vector3();
        }

        /**
         * Constructs a vertex with the specified coordinates
         * and index.
         */
        public Vertex(float x, float y, float z, int idx)
        {
            pnt = new Vector3(x, y, z);
            index = idx;
        }
    }
}