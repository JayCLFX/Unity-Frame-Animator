using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

    public class Frame_Animator : MonoBehaviour
    {
        //Base Definitions
        [Space(20)]
        [Header("Definitions")]
        [Tooltip("For Material and Image Renderer")]
        [Space(10)][SerializeField] public Texture2D[] textureFrames;
        [Tooltip("For Sprite Renderer")]
        [Space(5)][SerializeField] public Sprite[] spriteFrames;

        //Settings
        [Space(5)]
        [Header("Settings")]
        [Space(10)]
        [Tooltip("Choose which Renderer to Use")]
        [SerializeField] Type usedRenderer = new Type();
        [Space(5)]
        [Tooltip("How fast the Animation is gonna be (25f Standart)")]
        [SerializeField] public float framesPerSecond = 25f;
        [Space(5)]
        [Tooltip("If the Script should Start it at the First Frame or manually by another Script")]
        [SerializeField] public bool startAutomatic;
        [Space(5)]
        [Tooltip("If the Script should Switch Scene after the Animation")]
        [SerializeField] public bool switchSceneIfFinished;
        [Space(5)]
        [Tooltip("Scene to Switch to when 'Switch_Scene_If_Finished' is true")]
        [SerializeField] public int sceneNumber;
        [Space(5)]
        [Tooltip("If the Script should loop the Animation forever")]
        [SerializeField] public bool loopAnimation;
        [Space(5)]
        [Tooltip("Defines if there should be a Sound Played while playing the Animation")]
        [SerializeField] public bool useSound;
        [Space(5)][SerializeField] public AudioSource soundAudioSource;
        [Space(5)][SerializeField] public AudioClip soundAudioClip;


        //Internal Functions
        [HideInInspector][SerializeField] private int currentFrame;
        [HideInInspector][SerializeField] private bool playedAnimation;
        [HideInInspector][SerializeField] private static bool startAnimation;
        [HideInInspector][SerializeField] private bool loadSceneOnce;

        //Internal Renderers
        [HideInInspector][SerializeField] private RawImage imageRenderer;
        [HideInInspector][SerializeField] private SpriteRenderer spriteRenderer;
        [HideInInspector][SerializeField] private Renderer materialRenderer;


        //Used if "Start_Automatic" is not used (Call with other Script)
        public static void Start_Animaton()
        {
            startAnimation = true;
        }

        void Start ()
        {
            //Safety
            currentFrame = 0; playedAnimation = false; startAnimation = false; loadSceneOnce = false;

            //Initialize
            if (useSound && !soundAudioSource | !soundAudioClip) {
                soundAudioSource = GetComponent<AudioSource>();
                soundAudioClip = GetComponent<AudioClip>();
            }
            bool checkRenderer = true;
            switch (usedRenderer)
            {
                case Type.SpriteRenderer: 
                    spriteRenderer = GetComponent<SpriteRenderer>();
                    if (!spriteRenderer | spriteFrames.Length == 0) { checkRenderer = false; }
                    break;

                case Type.RawImage:
                    imageRenderer = GetComponent<RawImage>();
                    if (!imageRenderer | textureFrames.Length == 0) { checkRenderer = false; }
                    break;

                case Type.Material:
                    materialRenderer = GetComponent<Renderer>();
                    if (!materialRenderer | textureFrames.Length == 0) { checkRenderer = false; }
                    break;

            }

            //Check if Values are Legal
            bool checkFPS = framesPerSecond != 0 ? true : false;
            bool checkSound = soundAudioClip && soundAudioSource ? true : false;
            bool checkBuildIndex = SceneUtility.GetScenePathByBuildIndex(sceneNumber).Length > 0 ? true : false;
            if (!checkFPS | !checkSound | !checkRenderer | !checkBuildIndex)
            {
                if (!checkFPS) {
                    Debug.LogError($"'{gameObject.name}' has invalid values in Script: 'Frames Per Second Not Set'", transform); 
                    enabled = false;
                }

                if (useSound && !checkSound) {
                    Debug.LogError($"'{gameObject.name}' has invalid values in Script: 'AudioSource or AudioClip Not Set'", transform); 
                    enabled = false;
                }

                if (!checkRenderer) {
                    Debug.LogError($"'{gameObject.name}' has invalid values in Script: 'No Frames defined for youre specified Renderer:  {usedRenderer}'", transform);
                    enabled = false;
                }

                if (!checkBuildIndex) {
                    Debug.LogError($"'{gameObject.name}' has invalid values in Script: 'Unity Build Index dosent contain the specified Number:  {sceneNumber}'", transform);
                    enabled = false;
                }
            }

            //Check how Animation is triggered
            startAnimation = startAutomatic == true ? true : false;
            return;
        }

        private void Update()
        {
            if (startAnimation)
            {
                switch (usedRenderer)
                {
                    //For Sprite Renderer
                    case Type.SpriteRenderer:
                        int spriteIndex = (int)(Time.time * framesPerSecond) % spriteFrames.Length;
                        if (!loopAnimation) {

                            if (spriteIndex == spriteFrames.Length - 1) { playedAnimation = true; this.enabled = false; }
                            if (spriteIndex != currentFrame && spriteIndex != spriteFrames.Length - 1 && !playedAnimation)
                            {
                                currentFrame = spriteIndex;
                                spriteRenderer.sprite = spriteFrames[currentFrame];
                            }
                        }
                        else {
                            if (spriteIndex != currentFrame)
                            {
                                currentFrame = spriteIndex;
                                spriteRenderer.sprite = spriteFrames[currentFrame];
                            }
                        }
                        //Switch Frame if Activated in Settings
                        if (spriteIndex == spriteFrames.Length - 1
                            && switchSceneIfFinished && !loadSceneOnce)
                        {
                            StartCoroutine(LoadScene());
                            loadSceneOnce = true;
                        }
                        break;

                    //For Image Renderer
                    case Type.RawImage:
                        int imageIndex = (int)(Time.time * framesPerSecond) % textureFrames.Length;
                        if (!loopAnimation) {

                            if (imageIndex == textureFrames.Length - 1) { playedAnimation = true; this.enabled = false; }
                            if (imageIndex != currentFrame && imageIndex != textureFrames.Length && !playedAnimation)
                            {
                                currentFrame = imageIndex;
                                imageRenderer.texture = textureFrames[currentFrame];
                            }
                        }
                        else {
                            if (imageIndex != currentFrame)
                            {
                                currentFrame = imageIndex;
                                imageRenderer.texture = textureFrames[currentFrame];
                            }
                        }
                        //Switch Frame if Activated in Settings
                        if (imageIndex == textureFrames.Length - 1
                            && switchSceneIfFinished && !loadSceneOnce)
                        {
                            StartCoroutine(LoadScene());
                            loadSceneOnce = true;
                        }
                        break;

                    //For Material Renderer
                    case Type.Material:
                        Material rendererMaterial = materialRenderer.material;
                        int materialIndex = (int)(Time.time * framesPerSecond) % textureFrames.Length;
                        if (!loopAnimation) {

                            if (materialIndex == textureFrames.Length - 1) { playedAnimation = true; this.enabled = false; }
                            if (materialIndex != currentFrame && materialIndex != textureFrames.Length && !playedAnimation)
                            {
                                currentFrame = materialIndex;
                                rendererMaterial.mainTexture = textureFrames[currentFrame];
                            }
                        }
                        else {
                            currentFrame = materialIndex;
                            rendererMaterial.mainTexture = textureFrames[currentFrame];
                        }
                        //Switch Frame if Activated in Settings
                        if (materialIndex == textureFrames.Length - 1
                            && switchSceneIfFinished && !loadSceneOnce)
                        {
                            StartCoroutine(LoadScene());
                            loadSceneOnce = true;
                        }
                        break;
                }
            }
        }

        IEnumerator LoadScene()
        {
            AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneNumber, LoadSceneMode.Single);

            while (!loadScene.isDone)
            {
                yield return null;
            }
        }
    }

public enum Type
{
    SpriteRenderer,
    RawImage,
    Material
};
