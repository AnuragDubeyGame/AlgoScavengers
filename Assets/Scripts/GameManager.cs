using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;
	public float turnDelay = .1f;
	public static GameManager instance = null;
	public BoardManager boardScript;
	public int playerFoodPoints = 100;
    public float restartLevelDelayAfterDeath = 3f;
    [HideInInspector] public bool playersTurn = true;

	private Text levelText;
	private GameObject levelImage;
	private int level = 1;
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
		levelText.text = "Day " + level;
		levelImage.SetActive(true);
		Invoke ("HideLevelImage", levelStartDelay);

		enemies.Clear();
		boardScript.SetupScene(level);
	}

	IEnumerator ResetGameManager()
	{
		print("Restarting Game");
		doingSetup = true;
        level = 0;
		enemies.Clear();
        yield return new WaitForSeconds (3);
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
		yield return new WaitForSeconds (1);
		playerFoodPoints = 100;
		FindObjectOfType<Player>().foodText.text = "Food: " + playerFoodPoints;
		FindObjectOfType<Player>().SetFood(playerFoodPoints);
		enemiesMoving = false;
		playersTurn = true;
        doingSetup = false;
		print("Restarted Game");
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


/*
 API TO HOST NFT = eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySW5mb3JtYXRpb24iOnsiaWQiOiJjMWMyMzdiNy0zMjljLTQ5N2QtOWY5Mi1jMDRmYjc4MTQwMDciLCJlbWFpbCI6ImFudXJhZ3RkMTJAZ21haWwuY29tIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsInBpbl9wb2xpY3kiOnsicmVnaW9ucyI6W3siaWQiOiJGUkExIiwiZGVzaXJlZFJlcGxpY2F0aW9uQ291bnQiOjF9LHsiaWQiOiJOWUMxIiwiZGVzaXJlZFJlcGxpY2F0aW9uQ291bnQiOjF9XSwidmVyc2lvbiI6MX0sIm1mYV9lbmFibGVkIjpmYWxzZSwic3RhdHVzIjoiQUNUSVZFIn0sImF1dGhlbnRpY2F0aW9uVHlwZSI6InNjb3BlZEtleSIsInNjb3BlZEtleUtleSI6IjA2ZmEyYjU4ZjdhZGZlZWQzMGMyIiwic2NvcGVkS2V5U2VjcmV0IjoiMjMxNDM5MWU1MmRjYzhmMTIzZGIxOTMxZGM1Nzg0ZmZiYjhjNDAzMzcxMGNkNjk1NWEwOWE4Y2JmZGJmZmEzMyIsImlhdCI6MTY5OTAyNTkyOX0.KTlLzC8DO_ETAxC7iKnMKEIq6_4cr-8gv9Y27Tf2Mkg


const axios = require('axios')
const FormData = require('form-data')
const fs = require('fs')
const JWT = eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySW5mb3JtYXRpb24iOnsiaWQiOiJjMWMyMzdiNy0zMjljLTQ5N2QtOWY5Mi1jMDRmYjc4MTQwMDciLCJlbWFpbCI6ImFudXJhZ3RkMTJAZ21haWwuY29tIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsInBpbl9wb2xpY3kiOnsicmVnaW9ucyI6W3siaWQiOiJGUkExIiwiZGVzaXJlZFJlcGxpY2F0aW9uQ291bnQiOjF9LHsiaWQiOiJOWUMxIiwiZGVzaXJlZFJlcGxpY2F0aW9uQ291bnQiOjF9XSwidmVyc2lvbiI6MX0sIm1mYV9lbmFibGVkIjpmYWxzZSwic3RhdHVzIjoiQUNUSVZFIn0sImF1dGhlbnRpY2F0aW9uVHlwZSI6InNjb3BlZEtleSIsInNjb3BlZEtleUtleSI6IjA2ZmEyYjU4ZjdhZGZlZWQzMGMyIiwic2NvcGVkS2V5U2VjcmV0IjoiMjMxNDM5MWU1MmRjYzhmMTIzZGIxOTMxZGM1Nzg0ZmZiYjhjNDAzMzcxMGNkNjk1NWEwOWE4Y2JmZGJmZmEzMyIsImlhdCI6MTY5OTAyNTkyOX0.KTlLzC8DO_ETAxC7iKnMKEIq6_4cr-8gv9Y27Tf2Mkg

const pinFileToIPFS = async () => {
    const formData = new FormData();
    const src = "path/to/file.png";
    
    const file = fs.createReadStream(src)
    formData.append('file', file)
    
    const pinataMetadata = JSON.stringify({
      name: 'File name',
    });
    formData.append('pinataMetadata', pinataMetadata);
    
    const pinataOptions = JSON.stringify({
      cidVersion: 0,
    })
    formData.append('pinataOptions', pinataOptions);

    try{
      const res = await axios.post("https://api.pinata.cloud/pinning/pinFileToIPFS", formData, {
        maxBodyLength: "Infinity",
        headers: {
          'Content-Type': `multipart/form-data; boundary=${formData._boundary}`,
          'Authorization': `Bearer ${JWT}`
        }
      });
      console.log(res.data);
    } catch (error) {
      console.log(error);
    }
}
pinFileToIPFS()

 
 */

