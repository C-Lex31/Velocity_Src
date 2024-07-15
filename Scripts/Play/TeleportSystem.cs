using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeleportSystem : MonoBehaviour
{
    public static TeleportSystem instance;

    [SerializeField] private List<Theme> themes;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float warpDuration = 1.0f;
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

        Player.instance.Teleport(true);
        this.po = po;
        yield return new WaitForSeconds(0.7f);
        warpMaterial.SetFloat("_WarpStrength", 0);
        float elapsedTime = 0;

        while (elapsedTime < warpDuration)
        {
            elapsedTime += Time.deltaTime;

            warpMaterial.SetFloat("_WarpStrength", elapsedTime / warpDuration);

            yield return null;
        }

        // Change the theme
        int themeIndex = GetNextThemeIndex();
        ApplyTheme(themes[themeIndex]);
        po.gameObject.SetActive(false);
        // End the warp effect
        elapsedTime = warpDuration;
        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;
            warpMaterial.SetFloat("_WarpStrength", elapsedTime / warpDuration);
            yield return null;
        }
        warpMaterial.SetFloat("_WarpStrength", 0);
        po.gameObject.SetActive(true);
        this.po.Appear();

        yield return new WaitForSeconds(0.3f);//slight delay to allow portal to open fully
        Player.instance.Teleport(false);
    }

    private void ApplyTheme(Theme theme)
    {


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
        OnThemeChanged?.Invoke(theme, ref po);
    }
}
