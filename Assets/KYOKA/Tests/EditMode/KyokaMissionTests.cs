#if UNITY_EDITOR
using NUnit.Framework;
using Kyoka.Missions;

public class KyokaMissionTests
{
    [Test]
    public void MissionCompletesAtRequiredProgress()
    {
        var mission = new KyokaMission { requiredProgress = 2 };
        mission.AddProgress();
        Assert.IsFalse(mission.IsComplete);
        mission.AddProgress();
        Assert.IsTrue(mission.IsComplete);
    }

    [Test]
    public void MissionProgressDoesNotExceedRequirement()
    {
        var mission = new KyokaMission { requiredProgress = 2 };
        mission.AddProgress(20);
        Assert.AreEqual(2, mission.currentProgress);
    }
}
#endif
