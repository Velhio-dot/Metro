using UnityEngine;

namespace Metro.Utils
{
    public static class Utils
    {
        public static Vector3 GetRadnomDir()
        {
            return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }
    } 
}
