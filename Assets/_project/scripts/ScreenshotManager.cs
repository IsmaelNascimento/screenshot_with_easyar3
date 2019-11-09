using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NatShareU;
using UnityEngine;
using UnityEngine.Android;

public class ScreenshotManager : MonoBehaviour
{
    #region VARIABLES

    private static ScreenshotManager instance;
    public static ScreenshotManager Instance
    {
        get
        {
            if (instance == null)
                instance = new GameObject().AddComponent<ScreenshotManager>();

            return instance;
        }
    }

    [SerializeField] private List<GameObject> gameObjectsForDisable;
    private Texture2D screenshotTexture2D;

    #endregion

    #region MONOBEHAVIOUR_METHODS

    private void Start()
    {
        VerifyPermissionSaveScreenshot();
    }

    #endregion

    #region PUBLIC_METHODS

    [ContextMenu("OnButtonScreenshotClicked")]
    public void OnButtonScreenshotClicked(Action afterScreenshot = null)
    {
        StartCoroutine(ScreenshotSystem_Coroutine(afterScreenshot));
        Debug.Log("OnButtonScreenshotClicked");
    }

    [ContextMenu("OnButtonSaveScreenshotClicked")]
    public void OnButtonSaveScreenshotClicked()
    {
        Debug.Log("OnButtonSaveScreenshotClicked");
        if (!File.Exists(GetPathScreenshot()))
        {
            Debug.LogError("Screenshot not exist\nPath: " + GetPathScreenshot());
            return;
        }

        Debug.Log("Screenshot\nPath: " + GetPathScreenshot());
        byte[] screenshotFile = File.ReadAllBytes(GetPathScreenshot());
        screenshotTexture2D = new Texture2D(Screen.width, Screen.height);
        screenshotTexture2D.LoadImage(screenshotFile);
        NatShare.SaveToCameraRoll(screenshotTexture2D);
    }

    public void OnButtonShareScreenshotClicked()
    {
        NatShare.Share(screenshotTexture2D);
        Debug.Log("OnButtonShareScreenshotClicked");
    }

    [ContextMenu("OnButtonScreenshotAndSaveClicked")]
    public void OnButtonScreenshotAndSaveClicked()
    {
        OnButtonScreenshotClicked(OnButtonSaveScreenshotClicked);
    }

    #endregion

    #region PRIVATE_METHODS

    private string NameScreenshot()
    {
        if (Application.isEditor)
        {
            int count = Directory.GetFiles(Application.persistentDataPath).Length;
            return $"{Application.persistentDataPath}/{Application.productName}-{count}.png";
        }
        else
        {
            int count = Directory.GetFiles(Application.persistentDataPath).Length;
            return $"{Application.productName}-{count}.png";
        }
    }

    private string GetPathScreenshot()
    {
        if (Application.isEditor)
        {
            int count = Directory.GetFiles(Application.persistentDataPath).Length - 1;
            return $"{Application.persistentDataPath}/{Application.productName}-{count}.png";
        }
        else
        {
            int count = Directory.GetFiles(Application.persistentDataPath).Length - 1;
            return Path.Combine(Application.persistentDataPath, $"{Application.productName}-{count}.png");
        }
    }

    private void VerifyPermissionSaveScreenshot()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Debug.Log("Ask permission for ExternalStorageWrite");
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif
    }

    #endregion

    #region COROUTINES

    private IEnumerator ScreenshotSystem_Coroutine(Action afterScreenshot = null)
    {
        gameObjectsForDisable.ForEach(gameObject => gameObject.SetActive(false));
        yield return new WaitForSeconds(.1f);
        ScreenCapture.CaptureScreenshot(NameScreenshot());
        yield return new WaitForSeconds(.1f);
        gameObjectsForDisable.ForEach(gameObject => gameObject.SetActive(true));
        yield return new WaitForSeconds(.1f);
        afterScreenshot?.Invoke();
    }

    #endregion
}