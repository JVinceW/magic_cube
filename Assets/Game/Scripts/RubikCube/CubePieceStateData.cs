using System;

namespace Game.Scripts.RubikCube {
    [Serializable]
    public class CubePieceStateData {
        public int coordinatorX;
        public int coordinatorY;
        public int coordinatorZ;

        public float positionX;
        public float positionY;
        public float positionZ;

        public float rotX;
        public float rotY;
        public float rotZ;
    }
}