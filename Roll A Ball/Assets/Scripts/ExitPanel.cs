using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExitPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameTimer gameTimer;

    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameObject.Find("NetworkRoot").GetComponent<GameManager>();
    }

    private void OnEnable()
    {
        float time = gameTimer.GetElapsedTime();

        int minute = Mathf.FloorToInt(time / 60);
        int second = Mathf.FloorToInt(time % 60);

        timeText.text = minute.ToString("00") + " : " + second.ToString("00");
    }

    public void OnClickReturnButton()
    {
        Time.timeScale = 1f;
        gameManager.ReturnToStart();
    }

    public void OnClickExitButton()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}