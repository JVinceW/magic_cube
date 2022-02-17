using System;
using System.Collections.Generic;
using Game.Scripts.Common.Singleton;
using Game.Scripts.RubikCube;

namespace Game.Scripts.Common {
    [Serializable]
    public class PlayerLocalSaveData : LocalSaveSingleton<PlayerLocalSaveData> {
        public int LastPlayedRubickSize = 0;
        public List<CubePieceStateData> LastPlayedCubePieceStateDatas = new List<CubePieceStateData>();
    }
}