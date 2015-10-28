using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Vertex
    {
        private static int _index;

        public float Radius = 1;

        private Vector3 _position;
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                if (CurrentGameObject != null)
                {
                    CurrentGameObject.transform.position = Position;
                }
            }
        }

        public int Id { get; private set; }

        private readonly double _radiusPow2;

        public GameObject CurrentGameObject;

        public Vertex()
        {
            Id = _index++;
            _radiusPow2 = Math.Pow(Radius, 2);
        }

        public bool Collision(Vector3 point)
        {
            var delta = Math.Pow((Position.x - point.x), 2) + Math.Pow((Position.y - point.y), 2);

            if (delta <= _radiusPow2)
                return true;

            return false;
        }
    }
}
