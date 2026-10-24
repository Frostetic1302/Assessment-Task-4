using System.Collections;
using TMPro;
using UnityEngine;

public class RoundStart : MonoBehaviour
{
    public TMP_Text countdownText;
    public GameObject blocker;

    IEnumerator Start()
    {
        blocker.SetActive(true);
        countdownText.text = "3";
        yield return new WaitForSeconds(1);
        countdownText.text = "2";
        yield return new WaitForSeconds(1);
        countdownText.text = "1";
        yield return new WaitForSeconds(1);
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1);

        blocker.SetActive(false);
        countdownText.gameObject.SetActive(false);
        GameManager.instance.StartGame();
    }
}