using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerOfLondon : MonoBehaviour
{
    [Header("Game Configuration")]
    public LevelConfig[] levels;
    public GameObject[] pegs;         
    public GameObject ballPrefab;

    [Header("Sprites")]
    public Sprite redSprite;          
    public Sprite greenSprite;        
    public Sprite blueSprite;         
    public Sprite yellowSprite;       
    public Sprite purpleSprite;       

    [Header("UI References")]
    public Transform targetDisplay;   
    public GameObject targetSpritePrefab; 
    public Text feedbackText;         
    public Text movesText;            
    public Text timerText;            
    public Text scoreText;            
    public Button nextLevelButton;    
    public Button restartButton;      

    [Header("VFX")]
    public ParticleSystem winParticles;
    public ParticleSystem loseParticles;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("UI Settings")]
    public float feedbackDuration = 5f; 

    private List<List<GameObject>> balls = new List<List<GameObject>>();
    private int currentLevel = 0;
    private int movesLeft;
    private float timeLeft;
    private int totalScore = 0;
    private bool isGameActive = true;

    void Start()
    {
        
        for (int i = 0; i < pegs.Length; i++)
        {
            balls.Add(new List<GameObject>());
        }

        
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        
        winParticles.Stop(true);
        loseParticles.Stop(true);
        nextLevelButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false); 

        LoadLevel(currentLevel);
        StartCoroutine(GameTimer());

        
        nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        restartButton.onClick.AddListener(RestartGame);
    }

    void LoadLevel(int levelIndex)
    {
        ClearBalls();
        InitializeBalls(levels[levelIndex].startState);
        CreateTargetConfiguration(levels[levelIndex].targetState);
        movesLeft = levels[levelIndex].moveLimit;
        timeLeft = levels[levelIndex].timeLimit;
        UpdateUI();
    }

    void ClearBalls()
    {
        foreach (var peg in balls)
        {
            foreach (var ball in peg)
            {
                Destroy(ball);
            }
            peg.Clear();
        }
    }

    void InitializeBalls(List<Color> startColors)
    {
        for (int i = 0; i < startColors.Count; i++)
        {
            GameObject ball = Instantiate(ballPrefab, pegs[0].transform);
            ball.transform.localPosition = new Vector3(0, i * 0.5f, 0);
            ball.GetComponent<Renderer>().material.color = startColors[i];
            ball.AddComponent<BallInteraction>();
            balls[0].Add(ball);
        }
    }

    void CreateTargetConfiguration(List<Color> targetColors)
    {
        
        foreach (Transform child in targetDisplay)
        {
            Destroy(child.gameObject);
        }

        
        for (int i = 0; i < targetColors.Count; i++)
        {
            
            GameObject preview = Instantiate(targetSpritePrefab, targetDisplay);

            
            preview.SetActive(true);

            
            RectTransform rt = preview.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, i * -50f); 

            
            Image img = preview.GetComponent<Image>();
            img.sprite = GetSpriteForColor(targetColors[i]);
            img.color = targetColors[i]; 

            Debug.Log($"Создан спрайт для цвета: {targetColors[i]}");
        }
    }

    Sprite GetSpriteForColor(Color color)
    {
        if (color == Color.red) return redSprite;
        if (color == Color.green) return greenSprite;
        if (color == Color.blue) return blueSprite;
        if (color == Color.yellow) return yellowSprite;
        if (color == Color.magenta) return purpleSprite; 
        return null;
    }

    IEnumerator GameTimer()
    {
        while (isGameActive)
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                UpdateUI();
                yield return null;
            }
            else
            {
                EndGame(false);
                yield break;
            }
        }
    }

    public void MoveBall(GameObject ball, int newPegIndex)
    {
        if (!isGameActive) return;

        int oldPegIndex = FindPegIndex(ball);
        if (oldPegIndex == -1 || oldPegIndex == newPegIndex) return;
        if (!IsTopBall(ball, oldPegIndex)) return;

        balls[oldPegIndex].Remove(ball);
        balls[newPegIndex].Add(ball);

        Vector3 newPos = pegs[newPegIndex].transform.position;
        newPos.y += (balls[newPegIndex].Count - 1) * 0.5f;
        StartCoroutine(SmoothMove(ball.transform, newPos));

        movesLeft--;
        UpdateUI();
        CheckSolution();
    }

    IEnumerator SmoothMove(Transform ballTransform, Vector3 targetPos)
    {
        float duration = 0.5f;
        float elapsed = 0;
        Vector3 startPos = ballTransform.position;

        while (elapsed < duration)
        {
            ballTransform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        ballTransform.position = targetPos;
    }

    void CheckSolution()
    {
        List<Color> currentColors = GetPegColors(1);
        List<Color> targetColors = levels[currentLevel].targetState;

        bool isCorrect = currentColors.Count == targetColors.Count;
        for (int i = 0; i < currentColors.Count && isCorrect; i++)
        {
            if (currentColors[i] != targetColors[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            audioSource.PlayOneShot(winSound);
            winParticles.Play();
            totalScore += 10;
            ShowFeedback("Победа! +10 очков");
            nextLevelButton.gameObject.SetActive(true);
        }
        else if (movesLeft <= 0)
        {
            EndGame(false);
        }
    }

    void OnNextLevelButtonClicked()
    {
        winParticles.Stop();
        audioSource.Stop();
        if (currentLevel < levels.Length - 1)
        {
            currentLevel++;
            LoadLevel(currentLevel);
            nextLevelButton.gameObject.SetActive(false);
        }
        else
        {
            EndGame(true);
        }
    }

    void EndGame(bool isWin)
    {
        isGameActive = false;
        if (isWin)
        {
            ShowFeedback($"Общий счет: {totalScore}");
        }
        else
        {
            audioSource.PlayOneShot(loseSound);
            loseParticles.Play();
            ShowFeedback("Поражение! Попробуйте снова");
            restartButton.gameObject.SetActive(true);
        }
    }

    public void RestartGame()
    {
        loseParticles.Stop();
        audioSource.Stop();
        currentLevel = 0;
        totalScore = 0;
        LoadLevel(currentLevel);
        restartButton.gameObject.SetActive(false);
        isGameActive = true;
        StartCoroutine(GameTimer());
    }

    void ShowFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        StartCoroutine(HideFeedbackAfterDelay(feedbackDuration));
    }

    IEnumerator HideFeedbackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedbackText.gameObject.SetActive(false);
    }

    List<Color> GetPegColors(int pegIndex)
    {
        List<Color> colors = new List<Color>();
        if (pegIndex < 0 || pegIndex >= balls.Count) return colors;

        for (int i = balls[pegIndex].Count - 1; i >= 0; i--)
        {
            colors.Add(balls[pegIndex][i].GetComponent<Renderer>().material.color);
        }
        return colors;
    }

    public int FindPegIndex(GameObject ball)
    {
        for (int i = 0; i < balls.Count; i++)
        {
            if (balls[i].Contains(ball)) return i;
        }
        return -1;
    }

    public bool IsTopBall(GameObject ball, int pegIndex)
    {
        if (pegIndex < 0 || pegIndex >= balls.Count) return false;
        return balls[pegIndex].Count > 0 && balls[pegIndex][balls[pegIndex].Count - 1] == ball;
    }

    void UpdateUI()
    {
        movesText.text = $"Ходов: {movesLeft}";
        timerText.text = $"Время: {Mathf.Max(0, timeLeft):F1}";
        scoreText.text = $"Очки: {totalScore}";
    }
}