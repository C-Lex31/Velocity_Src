using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeleportSystem : MonoBehaviour
{
    public static TeleportSystem instance;

    [SerializeField] private List<Theme> themes;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private ParticleSystem warpEffect;
    [SerializeField] private Transform themeBackground;
    public Material warpMaterial;
    private int currentThemeIndex = 0;
    private List<int> shuffledIndices;
    private Portal po;
    public delegate void ThemeChangedHandler(Theme newTheme, ref Portal po);
    public event ThemeChangedHandler OnThemeChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize the shuffled indices
        shuffledIndices = new List<int>();
        for (int i = 0; i < themes.Count; i++)
        {
            shuffledIndices.Add(i);
        }
        Shuffle(shuffledIndices);
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private int GetNextThemeIndex()
    {
        int nextIndex = shuffledIndices[currentThemeIndex];
        currentThemeIndex = (currentThemeIndex + 1) % shuffledIndices.Count;
        return nextIndex;
    }

    public void TriggerTeleport(Portal po)
    {
        StartCoroutine(TeleportCoroutine(po));
    }

    private IEnumerator TeleportCoroutine(Portal po)
    {
        // Play warp effect
        /*warpEffect.Play();

        // Fade to black
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }*/
        // Start the warp effect
        Player.instance.Teleport(true);
        this.po =po;
        yield return new WaitForSeconds(0.7f);
        warpMaterial.SetFloat("_WarpStrength", 0);
        float elapsedTime = 0;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            warpMaterial.SetFloat("_WarpStrength", elapsedTime / fadeDuration);

            yield return null;
        }

        // Change the theme
        int themeIndex = GetNextThemeIndex();
        
        ApplyTheme(themes[themeIndex]);

        // Fade back in
        /*elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }*/
        // End the warp effect
        elapsedTime = fadeDuration;
        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;
            warpMaterial.SetFloat("_WarpStrength", elapsedTime / fadeDuration);
            yield return null;
        }
        this.po.Appear();
        warpMaterial.SetFloat("_WarpStrength", 0);
        Player.instance.Teleport(false);
    }

    private void ApplyTheme(Theme theme)
    {
        // Change the background

        //  Deactivate all backgrounds
        foreach (Transform background in themeBackground)
        {
            background.gameObject.SetActive(false);
        }
        //Activate required background
        themeBackground.Find(theme.themeName).gameObject.SetActive(true);
        // Change the skybox
        RenderSettings.skybox = theme.skyboxMaterial;

        // Change the music
        if (theme.themeMusic != null)
        {
            //SoundManager.Instance.Play(theme.themeMusic);
        }



        // Notify LevelGenerator about the theme change
        OnThemeChanged?.Invoke(theme, ref po );
    }
}
