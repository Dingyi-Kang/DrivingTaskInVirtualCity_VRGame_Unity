using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryScript : MonoBehaviour
{
    public AudioSource boxPickup;
    public AudioSource boxDropoff;
    private GameObject boxes;
    private GameObject[] deliveryPoints;
    private int randNum;
    private bool first = true;

    // Start is called before the first frame update
    void Start()
    {
        //Get all possible delivery points, and randomly assign one to start
        deliveryPoints = GameObject.FindGameObjectsWithTag("Delivery");
        randNum = Random.Range(0, deliveryPoints.Length);
        transform.position = deliveryPoints[randNum].transform.position;
    }


    private void OnTriggerEnter(Collider other)
    {
        //When the player collides with the delivery point...
        if (first && other.gameObject.tag == "player")
        {
            //Assign a random new delivery point
            int oldNum = randNum;
            while (oldNum == randNum)
                randNum = Random.Range(0, deliveryPoints.Length - 1);
            transform.position = deliveryPoints[randNum].transform.position;

            //then pick up/drop off the package...
            GameObject[] children = GameObjectHelper.GetDirectChildren(other.gameObject);
            if (boxes == null)
                foreach (GameObject child in children)
                    if (child.name == "Boxes")
                    {
                        boxes = child;
                        break;
                    }
            if (boxes != null)
            {
                boxes.SetActive(!boxes.activeSelf);
                if (boxes.activeSelf)
                    boxPickup.Play();
                else
                    boxDropoff.Play();
            }
            else
                boxDropoff.Play();
            
            first = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!first && other.gameObject.tag == "player")
            first = true;
    }
}
