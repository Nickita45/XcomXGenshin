public static class RandomExtensions 
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxValue">Max value in procent</param>
    /// <returns>Return true if hited, false if miss</returns>
    public static bool GetChance(int maxValue) => UnityEngine.Random.Range(0,101) <= maxValue;

}
