using System;

namespace Game.Scripts.RubikCube {
    [Serializable]
    public class CubePieceStateData {
        public float coordinatorX;
        public float coordinatorY;
        public float coordinatorZ;

        public float positionX;
        public float positionY;
        public float positionZ;

        public float rotX;
        public float rotY;
        public float rotZ;
    }
}