using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DJ : MonoBehaviour
{
    public int touchCount = 0;
    void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.collider.CompareTag("Guard") || collision.collider.CompareTag("Clients") )
        {
            //Debug.Log("me golpearon");
            touchCount++;
            if (touchCount >= 3)
            {
                SoundManager.Instance.ChangeMusic();
                touchCount = 0;
            }
        }
    }
}
