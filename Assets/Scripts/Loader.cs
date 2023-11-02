using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	public GameObject gameManager;

	void Awake() {
		Application.targetFrameRate = 60;
		if (GameManager.instance == null) {
			Instantiate(gameManager);
		}
	}
}
