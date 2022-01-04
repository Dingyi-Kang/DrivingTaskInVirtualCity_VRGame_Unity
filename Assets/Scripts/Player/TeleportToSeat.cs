using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportToSeat : MonoBehaviour
{

    private Vector3 disToSeat;
    public GameObject player;

    public InputActionReference leftHand;
    public InputActionReference rightHand;
    public InputActionAsset asset;

    // Start is called before the first frame update
    void Start()
    {
        //Get the controls
        if(asset != null)
            asset.Enable();
    }

    // Update is called once per frame
    void Update()
    {

        //Once the buttons are both pressed simultaneously, teleport the player 
        if (leftHand.action.ReadValue<float>() == 1 && rightHand.action.ReadValue<float>() == 1)
            RecenterPlayerPosition();
    }

    void RecenterPlayerPosition()
    {
        //Get the distance from the seat to the player...
        disToSeat.x = transform.position.x - player.transform.position.x;
        disToSeat.z = transform.position.z - player.transform.position.z;
        //And then add that to the player's position, teleporting them to the correct position
        player.transform.parent.transform.position += new Vector3(disToSeat.x, 0f, disToSeat.z);
    }
}
