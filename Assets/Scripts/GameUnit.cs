using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUnit : MonoBehaviour
{
	private Vector2 pos;
	private int hpMax, hp, damage;
	private Transform t;
	private GameController gc;

	public GameUnit enemy;
	private RectTransform infoBox;
	private RectTransform infoBoxHP;
	private Text infoBoxTxt;

	// return position of player
	public Vector2 Pos  {
		get {
			return pos;
		}
	}

	// setup player params
	public void Setup (int hp, int damage, Vector2 pos, GameController gc)
	{
		this.pos = pos;
		this.gc = gc;
		this.hp = hp;
		this.hpMax = hp;
		this.damage = damage;

		t = transform;
		SetPosition(pos);

		// instantiate UI info
		infoBox = Instantiate(gc.InfoBox).GetComponent<RectTransform>();
		infoBox.SetParent(GameObject.Find("/UI/Icons").transform);
		infoBoxTxt = infoBox.Find("text/hp").GetComponent<Text>();
		infoBoxHP = infoBox.Find("hp").GetComponent<RectTransform>();
		infoBox.Find("text").GetComponent<Image>().color = GetComponent<MeshRenderer>().GetComponent<Renderer>().material.color;
		infoBox.Find("text/dmg").GetComponent<Text>().text = string.Format("dmg : {0}", damage);
		UpdateInfoBox();
	}

	// update UI
	void UpdateInfoBox () {
		infoBoxTxt.text = hp.ToString();
		infoBoxHP.localScale = new Vector3((float)hp / (float)hpMax, 1, 1);
	}

	// set player to position
	void SetPosition (Vector2 pos) {
		float delta = -gc.FieldSize / 2 + gc.UnitSize / 2f;
		t.localPosition = new Vector3(delta + pos.x, 0, delta + pos.y);
	}

	// setup enemy ref
	public void SetEnemy (GameUnit enemy)
	{
		this.enemy = enemy;
	}

	// called from GameController each move
	public void Run ()
	{
		if (TooClose(Pos, enemy.Pos)) {
			// if too close - Attack!!!
			StartCoroutine(AnimAttack());
		} else {
			// or move if far away
			Vector2 posTo = GetNewPosition();
			StartCoroutine(AnimMove(posTo));
		}
	}

	// enemy called this for damage
	bool MakeDamage (int damage)
	{
		this.hp = Mathf.Max(0, this.hp - damage);
		UpdateInfoBox();

		if (this.hp > 0)
			// i'm not dead!
			StartCoroutine(AnimFall());
		else
			// i'm dead
			StartCoroutine(AnimDie());

		return this.hp <= 0;
	}

	// calc new position for move
	Vector2 GetNewPosition ()
	{
		Vector2 dist = enemy.Pos - Pos;

		Vector2 direction = Vector2.zero;
		if (dist.x < 0)
			direction.x = -1;
		else
		if (dist.x > 0)
			direction.x = 1;

		if (dist.y < 0)
			direction.y = -1;
		else
		if (dist.y > 0)
			direction.y = 1;

		return pos + new Vector2(direction.x, direction.y);
	}

	// check if too close
	bool TooClose (Vector2 pos1, Vector2 pos2)
	{
		return (
			Mathf.Abs(pos1.x - pos2.x) <= 1 &&
			Mathf.Abs(pos1.y - pos2.y) <= 1
		);
	}

	// move anim
	IEnumerator AnimMove (Vector2 posTo)
	{
		// print(this);
		Vector2 posFrom = pos;

		float tmr = .0f;
		while (tmr < 1) {
			tmr += Time.deltaTime;
			SetPosition(Vector2.Lerp(posFrom, posTo, tmr));
			yield return null;
		}
		pos = posTo;
		gc.NextMove();
		// print("stop move");
	}

	// attack anim
	IEnumerator AnimAttack ()
	{
		bool fatalDamage = enemy.MakeDamage(damage);
		float tmr = .0f;
		while (tmr < 1) {
			tmr += Time.deltaTime * 2;
			float scale = 1 + Mathf.Sin(tmr * Mathf.PI) * .2f;
			t.localScale = Vector3.one * scale;
			yield return null;
		}
		yield return new WaitForSeconds(.5f);
		t.localScale = Vector3.one;

		if (fatalDamage)
			gc.GameOver(enemy);
		else
			gc.NextMove();
	}

	// hurt anim
	IEnumerator AnimFall ()
	{
		float tmr = .0f;
		while (tmr < 1) {
			tmr += Time.deltaTime * 2;
			float scale = 1 - Mathf.Sin(tmr * Mathf.PI) * .2f;
			t.localScale = Vector3.one * scale;
			yield return null;
		}
	}

	// die anim
	IEnumerator AnimDie ()
	{
		float tmr = .0f;
		while (tmr < 1) {
			tmr += Time.deltaTime * 1;
			t.localScale = Vector3.Lerp(Vector3.one, Vector2.zero, tmr);
			yield return null;
		}
		Destroy(gameObject);
	}

	// setup UI position
	void Update ()
	{
		infoBox.anchoredPosition3D = Camera.main.WorldToScreenPoint(t.position + new Vector3(0, 0, 2)) + new Vector3(-Screen.width / 2, -Screen.height / 2, 0);
	}

	// destroy UI before self destroy
	void OnDestroy ()
	{
		if (infoBox)
			Destroy(infoBox.gameObject);
	}
}