﻿using UnityEngine;
using System.Collections.Generic;

public class PrefabControl : MonoBehaviour {

	public List <GameObject> _weaponGOs = new List<GameObject>();
	public List <GameObject> _headGOs = new List<GameObject>();
	public List <GameObject> _chestGOs = new List<GameObject>();
	public List <GameObject> _legGOs = new List<GameObject>();
	public List <GameObject> _turretGOs = new List<GameObject>();

	void Start(){
		FindGos();
		EquipGOs();
	}

	void FindGos(){
		foreach (Transform child in GameObject.Find("Weapons").GetComponent<Transform>()){
			_weaponGOs.Add(child.gameObject);
		}
		foreach (Transform child in GameObject.Find("Helmets").GetComponent<Transform>()){
			_headGOs.Add(child.gameObject);
		}
		foreach (Transform child in GameObject.Find("Chest").GetComponent<Transform>()){
			_chestGOs.Add(child.gameObject);
		}
		foreach (Transform child in GameObject.Find("Legs").GetComponent<Transform>()){
			_legGOs.Add(child.gameObject);
		}
	}

	public void EquipGOs(){
		UpdateWeapons(_CombatManager._weaponDb._rangedDatabase.IndexOf(_CombatManager._equipRanged));
		UpdateHead(_CombatManager._armourDb._headDatabase.IndexOf(_CombatManager._headSlot));
		UpdateChest(_CombatManager._armourDb._chestDatabase.IndexOf(_CombatManager._chestSlot));
		UpdateLegs(_CombatManager._armourDb._legDatabase.IndexOf(_CombatManager._legSlot));
	}

	public void PreviewItems(int type, int index){
		switch(type){
			case 0:
			UpdateHead(index);
			break;
			case 1:			
			UpdateChest(index);
			break;
			case 2:			
			UpdateLegs(index);
			break;
			case 3:			
			UpdateWeapons(index);
			break;
		}		
	}

	void UpdateWeapons(int index){
		for (int i = 0; i < _weaponGOs.Count; i++){
			_weaponGOs[i].SetActive(index == i);
		}
	}

	void UpdateHead(int index){
		for (int i = 0; i < _headGOs.Count; i++){
			_headGOs[i].SetActive(index == i);
		}
	}

	void UpdateChest(int index){
		for (int i = 0; i < _chestGOs.Count; i++){
			_chestGOs[i].SetActive(index == i);
		}
	}

	void UpdateLegs(int index){
		for (int i = 0; i < _legGOs.Count; i++){
			_legGOs[i].SetActive(index == i);
		}
	}
}