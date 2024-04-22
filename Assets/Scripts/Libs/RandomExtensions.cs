public static class RandomExtensions 
{

    public static bool GetChance(int maxValue) => UnityEngine.Random.Range(0,101) <= maxValue;

}
