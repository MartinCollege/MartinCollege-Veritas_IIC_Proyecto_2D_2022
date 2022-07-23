using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector2 lastCheckPoint;
    public GameObject player;
    public float respawnTime;
    private bool _respawning = false;
    // Update is called once per frame
    void Update()
    {
        if (!player.activeInHierarchy && !_respawning)
        {
            _respawning = true;
            StartCoroutine(Respawning());
        }
    }

    IEnumerator Respawning()
    {
        yield return new WaitForSeconds(respawnTime);
        player.transform.position = lastCheckPoint;
        player.SetActive(true);
        _respawning = false;
    }
}
