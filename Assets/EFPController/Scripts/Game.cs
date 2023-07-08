using UnityEngine;

namespace EFPController {

    [DefaultExecutionOrder(-998)]
    public class Game : MonoBehaviour
    {

        public static Game instance;

        public static class LayerMask
        {
            public const int Default = 1 << Layer.Default;
            public const int Water = 1 << Layer.Water;
            public const int UI = 1 << Layer.UI;
            public const int Player = 1 << Layer.Player;
            public const int PlayerIgnore = 1 << Layer.PlayerIgnore;
            public const int PlayerCollide = 1 << Layer.PlayerCollide;
            public const int PlayerItems = 1 << Layer.PlayerItems;
        }

        public static class Layer
        {
            public const int Default = 0;
            public const int Water = 4;
            public const int UI = 5;
            public const int Player = 10;
            public const int PlayerIgnore = 9;
            public const int PlayerCollide = 8;
            public const int PlayerItems = 11;
        }

        public static class Tags
        {
            public const string Untagged = "Untagged";
            public const string Climbable = "Climbable";
            public const string Interactable = "Interactable";
        }

        private void Awake()
	    {
            instance = this;
            DontDestroyOnLoad(gameObject);
    #if UNITY_EDITOR
            UnityEditor.EditorWindow gameViewWindow = UnityEditor.EditorWindow.GetWindow(typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            gameViewWindow.Focus();
            Event fakeMouseClick = new Event()
            {
                button = 0,
                clickCount = 1,
                type = EventType.MouseDown,
                mousePosition = gameViewWindow.rootVisualElement.contentRect.center
            };
            gameViewWindow.SendEvent(fakeMouseClick);
    #endif
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

    }

}