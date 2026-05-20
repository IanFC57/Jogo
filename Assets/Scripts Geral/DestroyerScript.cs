using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerScript : MonoBehaviour {

    public Canvas GameOver;

    private void Start()
    {
        GameOver.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    private void OnTriggerEnter(Collider outro)
    {
        HandleTriggerEnter(outro.gameObject);
    }

#if !UNITY_ANDROID || UNITY_EDITOR
    private void OnTriggerEnter2D(Collider2D outro)
    {
        HandleTriggerEnter(outro.gameObject);
    }
#endif

    private void HandleTriggerEnter(GameObject outro)
    {
        if (outro.CompareTag("Player"))
        {
            Time.timeScale = 0;
            GameOver.gameObject.SetActive(true);
            return;
        }
        else
        {
            if (outro.transform.parent)
                Destroy(outro.transform.parent.gameObject);
            else
                Destroy(outro);
        }
	}
}
