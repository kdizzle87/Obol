using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControls_Combat : MonoBehaviour {

	public NavMeshAgent _agent;
	public Animator _anim;
	public Vector3 _objectPos;
	public Shooting _shooting;
	public LayerMask _layerMask;
	public Combat_UI _ui;
	public Transform _textSpawn;
	public GameObject _indicator;
	public SaveGame _saveGame;
	public PrefabControl _prefabs;
	public GameObject _activeTurret;

	public List <GameObject> _placedTurrets = new List <GameObject>();
	public int _maxTurrets = 5;

	public string _target;
	public int _npcIndex;
	public float _range;
	public float _healTimer = 0.1f;
	public bool _moving;
	public bool _firing;	
	public bool _moveToNPC;	
	public bool _placing;

	void Awake () {
		Spawn();
	}
	
	void Update () {
		if (!_ui._gameOver && !_placing) DetectInput();
		if (!_ui._gameOver && _placing) PositionTurret();
	}

	void Spawn(){
		_indicator = GameObject.Find("AimIndicator");
		_saveGame = GameObject.Find("Loader").GetComponent<SaveGame>();
		_ui = GameObject.Find("Combat UI").GetComponent<Combat_UI>();
		_anim = gameObject.GetComponentInChildren<Animator>();
		_agent = gameObject.GetComponent<NavMeshAgent>();
		_textSpawn = transform.Find("TextSpawn");		
		_shooting = transform.FindChild("Launcher").GetComponent<Shooting>();
		_prefabs = gameObject.GetComponent<PrefabControl>();		
		_agent.enabled = true;
	}

	void DetectInput(){
		if (!Input.GetMouseButton(1)) DetectMove();
		DetectAim();
		Heal();		
	}

	void DetectMove(){
		if (Input.GetMouseButton(0)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100f, _layerMask) && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()){
				_agent.speed = (_CombatManager._speed / 10.0f);		
				_anim.speed = 1.0f;		
				_anim.SetFloat("Speed", (_CombatManager._speed / 10.0f));
				if (hit.collider.tag == "Ground"){
					float dist = Vector3.Distance(hit.point, transform.position);
					if (dist > 1.0f){
						_agent.SetDestination(hit.point);
						_anim.SetBool("Running", true);
						_moving = true;
						_moveToNPC = false;
						if (_ui._uiOpen) _ui.CloseAllCanvases();						
					}
				}
				else if (hit.collider.tag == "NPC"){
					_agent.SetDestination(hit.transform.FindChild("PlayerPos").position);
					_moveToNPC = true;
					_anim.SetBool("Running", true);
					_moving = true;
					if (_ui._uiOpen) _ui.CloseAllCanvases();
					switch (hit.collider.name){
						case "Smith":
						_npcIndex = 1;
						break;
						case "Priest":
						_npcIndex = 2;
						break;
						case "Portal":
						_npcIndex = 3;
						break;
						case "Thief":
						_npcIndex = 4;
						break;
					}
				}		
			}
		}
		if (_moving){
			float dist = Vector3.Distance(transform.position, _agent.destination);
			if (dist <= 0.3f){
				Stop();
				if (_moveToNPC){
					_ui.OpenCanvas(_npcIndex);
					_moveToNPC = false;
				}
			}
		}
	}

	void Stop(){
		_agent.SetDestination(transform.position);
		_anim.SetBool("Running", false);
		_moving = false;
	}

	void DetectAim(){
		if (Input.GetMouseButton(1)){
			Stop();
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, 100f, _layerMask)){
				if (hit.collider.tag == "Ground" || hit.collider.tag == "Enemy" || hit.collider.tag == "Destructible"){
					if (hit.collider.tag == "Ground"){
						_indicator.SetActive(true);
						_indicator.transform.position = hit.point;	
					}
					else{
						_indicator.SetActive(false);
					}						
					_agent.SetDestination(transform.position);
					_anim.SetBool("Running", false);
					_moving = false;
					Quaternion newRotation = Quaternion.LookRotation(hit.point - transform.position);
					newRotation.x = 0f;
       				newRotation.z = 0f;
        			transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 10);
        			if (Input.GetMouseButton(0) && !_firing){
        				Shoot(hit.collider.gameObject, hit.point);
        			}
        			if (Input.GetMouseButtonDown(0) && !_firing){
        				Shoot(hit.collider.gameObject, hit.point);
        			}
				}			
			}			
		}		
		if (Input.GetMouseButtonUp(1)){
			_indicator.SetActive(false);

		}	
	}

	void Shoot(GameObject go, Vector3 target){
		var dist = Vector3.Distance(transform.position, target);	
		if (go.tag == "Ground"){
			if (dist <= 4.0f){
				_shooting.ShootStraight(target);
			}
			else{
        		_shooting.CalcVelocity(target);
        	}
        	StartCoroutine(FireRate());       					
        }
        else if (go.tag == "Resource" || go.tag == "Destructible"){
       		var h = 3 + go.transform.position.y;
       		var _aimTarget = new Vector3(go.transform.position.x, h, go.transform.position.z);
       		_shooting.CalcVelocity(_aimTarget);
       		StartCoroutine(FireRate());
       	}
        else if (go.tag == "Enemy"){
        	if (go.name == "Warden_Parent"){
        		if (dist <= 10.0f){
        			_shooting.ShootStraight(go.transform.GetChild(1).position);
        		}
        		else{
        			_shooting.CalcVelocity(go.transform.GetChild(1).position);
        		}
        		
        	}
        	else if (dist <= 5.0f){
        		_shooting.ShootStraight(go.transform.parent.position);
        	}
        	else{
        		_shooting.CalcVelocity(go.transform.parent.position);
        	}        	
        	StartCoroutine(FireRate());
        }
        _anim.SetBool("Attack", true);		
	}

	public IEnumerator FireRate(){
		_firing = true;	
		switch(_CombatManager._equipRanged._id){
			case 200:
			_anim.speed = 1.0f;
			break;
			case 201:
			_anim.speed = 0.17f;
			break;
			case 202:
			_anim.speed = 2.5f;
			break;
			case 203:
			_anim.speed = .71f;
			break;
		}	
		yield return new WaitForSeconds(_CombatManager._equipRanged._fireRate);
		_firing = false;
		_anim.SetBool("Attack", false);
	}

	void Heal(){
		if (_CombatManager._currentHealth < _CombatManager._maxHealth){
			_healTimer -= Time.deltaTime;
			if (_healTimer <= 0){
				_CombatManager._currentHealth += Mathf.CeilToInt((float) _CombatManager._maxHealth * 0.001f);
				if (_CombatManager._currentHealth > _CombatManager._maxHealth) _CombatManager._currentHealth = _CombatManager._maxHealth;
				_healTimer = 0.1f;
				_ui.UpdateUI();
			}			
		}		
	}
	
	public void BeenHit(int damage){
		if (!_ui._gameOver){
			var dam = Mathf.FloorToInt((float) damage);
			_CombatManager._currentHealth -= dam;
			_ui.DamageText(_textSpawn, dam, true);
			_ui.UpdateUI();		
			if (_CombatManager._currentHealth <= 0){
				_ui._gameOver = true;
				_agent.Stop();
				_anim.SetBool("Dead", true);
				StartCoroutine(Pause());
			}
		}		
	}

	public IEnumerator Pause(){
		yield return new WaitForSeconds(1.0f);
		_ui.LevelEnd(false);
	}

	public void ActivateTurretPos(int type){
		if (!_placing){
			_activeTurret = (type == 0) ? Instantiate (_prefabs._activeTurretA) : Instantiate (_prefabs._activeTurretB);
			_placing = true;
		}			
	}

	void PositionTurret(){
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, 100f, _layerMask)){
			if (hit.collider.tag == "Ground"){
				_activeTurret.SetActive(true);
				_activeTurret.transform.position = hit.point;
				if (Input.GetMouseButtonDown(0)){
					//Check List of turrets
					//If not max, place turret and add to list;
					var prefabTurret = (GameObject) Instantiate(_activeTurret, 	hit.point, Quaternion.identity);
					prefabTurret.SetActive(true);
					_placedTurrets.Add(prefabTurret);
					if (_placedTurrets.Count > _maxTurrets){
						Destroy(_placedTurrets[0]);
						_placedTurrets.RemoveAt(0);
					}
				}
				if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.T)) _placing = false;
			}
			else{
				_activeTurret.SetActive(false);
			}		
		}
		if (!_placing){
			Destroy(_activeTurret);
		}
	}
}