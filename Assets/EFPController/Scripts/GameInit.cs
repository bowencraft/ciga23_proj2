using UnityEngine;

namespace EFPController
{

    [DefaultExecutionOrder(-999)]
    public class GameInit
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            GameObject gamePrefab = Resources.Load<GameObject>("Game");
            if (gamePrefab != null) 
            {
                GameObject.Instantiate(gamePrefab);
            } else {
                Debug.LogError("Cant find Game prefab in Resources folders!");
            }
        }

    }

}