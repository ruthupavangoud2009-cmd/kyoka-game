using UnityEngine;

namespace Kyoka.Core
{
    [CreateAssetMenu(menuName = "KYOKA/Game Config")]
    public class KyokaGameConfig : ScriptableObject
    {
        public string gameTitle = "KYOKA";
        [Min(1)] public int startingHealth = 100;
        [Min(1)] public int startingEnergy = 100;
        public bool enableRuntimeIsland = true;
        public Color skyColor = new Color(0.28f, 0.48f, 0.72f);
    }
}
