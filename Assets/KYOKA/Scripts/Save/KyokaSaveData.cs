using System;

namespace Kyoka.Save
{
    [Serializable]
    public class KyokaSaveData
    {
        public float posX;
        public float posY;
        public float posZ;
        public bool hasSavedPosition;
        public int currency;
        public int playerLevel = 1;
    }
}
