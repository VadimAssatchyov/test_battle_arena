using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
	enum BattleState {
		menu,
		game,
		gameover
	}

	// prefabs
	public GameObject UnitRed, UnitBlue, FieldBlack, FieldWhite, InfoBox;

	// sceen objects
	public GameObject UIMainMenu, UIGameOver;

	// params
	public int FieldSize = 8;
	public int UnitSize = 1;
	public int MaxHP = 100;
	public int MinHP = 200;
	public int MaxDamage = 20;
	public int MinDamage = 50;

	private GameUnit player1, player2, playerActive;
	private BattleState battleState;
	private Transform fieldRoot;

	void Start()
	{
		ResetGame();
	}

	// reset scene
	void ResetGame ()
	{
		battleState = BattleState.menu;
		UIMainMenu.gameObject.SetActive(true);
		UIGameOver.gameObject.SetActive(false);
		gameObject.SetActive(false);
		transform.localRotation = Quaternion.identity;
	}

	// play button click/tap
	public void btnPlay ()
	{
		battleState = BattleState.game;
		UIMainMenu.gameObject.SetActive(false);
		gameObject.SetActive(true);
	}

	// setup game objects each time after PLAY button was clicked
	void OnEnable ()
	{
		if (battleState != BattleState.game)
			return;

		if (fieldRoot == null)
			CreateField();
		CreatePlayers();
		NextMove();
	}

	// destroy players objects after battle was finished
	void OnDisable ()
	{
		if (player1)
			Destroy(player1.gameObject);

		if (player2)
			Destroy(player2.gameObject);

	}

	// create chess field
	void CreateField ()
	{
		fieldRoot = transform.Find("Field");
		float delta = -FieldSize / 2 + UnitSize / 2f;
		for (var y = 0; y < FieldSize; y ++)
		{
			for (var x = 0; x < FieldSize; x ++)
			{
				Transform t = Instantiate((x + y) % 2 == 0 ? FieldBlack : FieldWhite).transform;
				t.SetParent(fieldRoot);
				t.localPosition = new Vector3(delta + x, -.6f, delta + y);
			}
		}
	}

	// create and setup two players
	void CreatePlayers ()
	{
		player1 = Instantiate(UnitRed).GetComponent<GameUnit>();
		player2 = Instantiate(UnitBlue).GetComponent<GameUnit>();

		Transform playersRoot = transform.Find("Units");
		player1.transform.SetParent(playersRoot);
		player2.transform.SetParent(playersRoot);

		player1.Setup(Random.Range(MinHP, MaxHP), Random.Range(MinDamage, MaxDamage), new Vector2(Random.Range(0, 4), Random.Range(0, FieldSize)), this);
		player2.Setup(Random.Range(MinHP, MaxHP), Random.Range(MinDamage, MaxDamage), new Vector2(Random.Range(5, 8), Random.Range(0, FieldSize)), this);

		player1.SetEnemy(player2);
		player2.SetEnemy(player1);

		playerActive = new GameUnit[] {player1, player2}[Random.Range(0, 2)];
	}

	// called each move
	public void NextMove ()
	{
		playerActive.Run();
		playerActive = playerActive == player1 ? player2 : player1;
	}

	// game over
	public void GameOver (GameUnit unit) {
		UIGameOver.gameObject.SetActive(true);
		Text message = UIGameOver.transform.Find("Message").GetComponent<Text>();
		message.color = unit.enemy.GetComponent<MeshRenderer>().GetComponent<Renderer>().material.color;
		message.text = string.Format("{0} WIN!", message.color == Color.red ? "RED" : "BLUE");
		battleState = BattleState.gameover;
		StartCoroutine(Timer());
	}

	// after 2 sec return to main menu
	IEnumerator Timer ()
	{
		yield return new WaitForSeconds(2);
		ResetGame();		
	}

	// rotation
	void Update ()
	{
		if (Input.GetMouseButton(0))
			transform.Rotate(new Vector3(0, -Input.GetAxis("Mouse X"), 0) * Time.deltaTime * 100f);
	}
}
