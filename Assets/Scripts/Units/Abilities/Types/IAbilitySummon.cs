
using System.Collections.Generic;

public interface IAbilitySummon
{
    public int RangeSummon();
    public string PathSummonedObject();
}

public interface IEnemyList
{
    public HashSet<Enemy> GetVisibleEnemies();
}

public interface IPercent
{
    public (int percent, ShelterType shelter) GetCalculationProcents(Unit enemy);
}
