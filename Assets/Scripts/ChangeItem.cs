﻿using UnityEngine;

namespace Assets.Scripts
{
    public class ChangeItem
    {
        public int VertexId { get; private set; }

        public Vector3 CurrentPosition { get; set; }

        public ChangeItem(int vertexId)
        {
            VertexId = vertexId;
        }
    }
}
