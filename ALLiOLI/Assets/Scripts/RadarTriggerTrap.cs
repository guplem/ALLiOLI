﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RadarTriggerTrap : MonoBehaviour
{
    
    private HashSet<Character> charactersInRadar = new HashSet<Character>();
    [SerializeField] private float maximumRadarDistance = 5;
    [SerializeField] private Transform pointOfMaxEffectivity;

    public SortedList<float, Character> GetCharactersInTrapRadar(Player exception) // Sorted by distance
    {
        SortedList<float, Character> playersInTrapRadar = new SortedList<float, Character>();
        
        foreach (Character character in charactersInRadar)
            if (!character.isDead && character.owner != exception)
                playersInTrapRadar.Add(GetRadarDistanceTo(character), character);

        return playersInTrapRadar;
    }

    public float GetRadarDistanceTo(Character character)
    {
        return Vector3.Distance(character.transform.position+Vector3.up, this.pointOfMaxEffectivity.position)/maximumRadarDistance;
        //return Vector3.Distance(character.cameraTarget.transform.position, this.gameObject.transform.position)/maximumRadarDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        Character character = other.GetComponent<Character>();
        if (character) charactersInRadar.Add(character);
    }

    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponent<Character>();
        if (character) charactersInRadar.Remove(character);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Selection.activeGameObject != transform.gameObject) return;
        
        Gizmos.color = new Color(1f, 0f, 1f, 0.25f);
        Gizmos.DrawSphere(pointOfMaxEffectivity.position, maximumRadarDistance);
    }
#endif

}
