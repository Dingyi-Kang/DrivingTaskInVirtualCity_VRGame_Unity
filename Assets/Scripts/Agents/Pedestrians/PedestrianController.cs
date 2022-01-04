
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;


public class PedestrianController : MonoBehaviour
{
    //get reference to the navMesshAgent
    public NavMeshAgent agent;

    //get access to the thirdPersonCharacterScript
    public ThirdPersonCharacter character;

    //properties of pedestrian
    public float maxWalkDistance = 1.0f;
    private bool depart = false;

    private void Start()
    {
        //disable the rotation update of agent
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if pedestrian hasn't departed 
        if (!depart)
        {
            //get a random direction
            Vector3 randomDirection = Random.insideUnitSphere * maxWalkDistance;
            //set the direction based on agent's current position
            randomDirection += transform.position;
            NavMeshHit hit;
            //get a random location in the baed nav mesh within the maxWalkDistance
            NavMesh.SamplePosition(randomDirection, out hit, maxWalkDistance, 1);
            //set the location as the destination of the agent
            agent.SetDestination(hit.position);
            //update the walking state of the agent
            depart = true;
        }
       
        //if agent has not arrived the destination, make the agent move in animatin in the third character script
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            character.Move(agent.desiredVelocity, false, false);
        }
        //if agent arrive the destination, it stops and its walking state gets update
        else
        {
            character.Move(Vector3.zero, false, false);
            depart = false;
        }


    }
    }
