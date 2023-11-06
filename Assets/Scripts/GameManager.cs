using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;
	public float turnDelay = .1f;
	public static GameManager instance = null;
	MainMenuManager mainMenuManager;
	public BoardManager boardScript;
	public int playerFoodPoints = 100;
    public float restartLevelDelayAfterDeath = 3f;
    [HideInInspector] public bool playersTurn = true;

	private Text levelText;
	private GameObject levelImage;
	private GameObject RestartGame_Btn;
	private GameObject ClaimBtn;
	private int level = 0;
	private List<Enemy> enemies;
	private bool enemiesMoving;
	private bool doingSetup;

	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);
		enemies = new List<Enemy> ();
		boardScript = GetComponent<BoardManager>();
		InitGame();
	}

	private void OnLevelWasLoaded(int index) {
		level++;
		InitGame();
	}

	void InitGame() {
		doingSetup = true;
        levelImage = GameObject.Find("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text>();
		RestartGame_Btn = GameObject.Find ("RestartBtn");
        ClaimBtn = GameObject.Find ("ClaimBtn");
		RestartGame_Btn.GetComponent<Button>().onClick.AddListener(delegate { RestartGame(); });
        ClaimBtn.GetComponent<Button>().onClick.AddListener(delegate { GoToMainMenu(); });
		RestartGame_Btn.SetActive(false);
		ClaimBtn.SetActive(false);
		levelText.text = "Day " + level;
		levelImage.SetActive(true);
		Invoke ("HideLevelImage", levelStartDelay);

		enemies.Clear();
		boardScript.SetupScene(level);
	}

	IEnumerator ResetGameManager()
	{
		print("GameOver");
		RestartGame_Btn.SetActive(true);
		ClaimBtn.SetActive(true);
		doingSetup = true;
		playersTurn = false;
        level = 0;
		enemies.Clear();
		yield return new WaitForSeconds (1);
		
	}
	
	public void RestartGame() {
        print("Restarting Game");
		RestartGame_Btn.SetActive(false);
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
        playerFoodPoints = 100;
        FindObjectOfType<Player>().foodText.text = "Food: " + playerFoodPoints;
        FindObjectOfType<Player>().SetFood(playerFoodPoints);
        doingSetup = false;
        enemiesMoving = false;
        playersTurn = true;
        print("Restarted Game");
    }


	public void GoToMainMenu()
	{
        RestartGame_Btn.SetActive(false);
        ClaimBtn.SetActive(false);
        doingSetup = true;
        playersTurn = false;
        level = 0;
        enemies.Clear();
        playerFoodPoints = 100;
        FindObjectOfType<Player>().foodText.text = "Food: " + playerFoodPoints;
        FindObjectOfType<Player>().SetFood(playerFoodPoints);
        doingSetup = false;
        enemiesMoving = false;
        playersTurn = true;
        StartCoroutine(smm());
	}
	private IEnumerator smm()
	{
		SceneManager.LoadScene("MainMenu");
		yield return new WaitForSeconds (0.35f);
		mainMenuManager = FindObjectOfType<MainMenuManager>();
		mainMenuManager.TokenToMint = level * 25;
		mainMenuManager.setPaymentAmount();
		mainMenuManager.GameBG.SetActive(false);
	}

	private void HideLevelImage() { 
		levelImage.SetActive(false);
		doingSetup = false;
	}

	public void GameOver() {
		levelText.text = "After " + level + " days, you starved.";
		levelImage.SetActive(true);
		StartCoroutine(ResetGameManager());
    }

    // Update is called once per frame
    void Update () {
		if (playersTurn || enemiesMoving || doingSetup)
			return;
		if(!doingSetup && !playersTurn)
		{
			StartCoroutine (MoveEnemies ());
		}
	}

	public void AddEnemeyToList(Enemy script) { 
		enemies.Add(script);
	}

	IEnumerator MoveEnemies() {
		enemiesMoving = true;
		yield return new WaitForSeconds (turnDelay);
		if (enemies.Count == 0) {
			yield return new WaitForSeconds(turnDelay);
		}

		for (int i = 0; i < enemies.Count; i++) {
			enemies[i].MoveEnemy();
			yield return new WaitForSeconds(enemies[i].moveTime);
		}

		enemiesMoving = false;
		playersTurn = true;
	}
}



