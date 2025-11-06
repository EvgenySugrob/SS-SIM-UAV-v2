using System.Collections;
using UnityEngine;

public class Logo : MonoBehaviour
{
    static public bool viewed;
    [SerializeField] GameObject mainMenu;

    public AnimationClip logoClip;


    private void OnEnable()
    {
        gameObject.SetActive(!viewed);

        if (!viewed)
            StartCoroutine(CLogo());
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mainMenu.SetActive(true);
    }



    private IEnumerator CLogo()
    {
        yield return new WaitForSeconds(logoClip.length+0.5f);
        gameObject.SetActive(false);
        viewed = true;
    }
}
