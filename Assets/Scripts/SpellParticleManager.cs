using UnityEngine;
using System.Collections;

public class SpellParticleManager : MonoBehaviour {

	public static Quaternion computeQuaternion(KreaturChip target, KreaturChip origin)
    {
        Quaternion result = new Quaternion(0.7f, 0.5f, 0.7f, -0.7f);

        if (origin == null)
            return result;
        else
        { //compute the direction for lance, and arrow
          //set speed for particle?

            Vector3 particleDirection = target.transform.position - origin.transform.position;
            Quaternion direction = Quaternion.LookRotation(particleDirection); 
                       
            return direction;
        }
    }

    public static void SpawnSpellParticles(GameObject particles, int duration, KreaturChip target, KreaturChip origin = null)
    {
        if (particles == null)
            return;

        Quaternion rotation = computeQuaternion(target, origin);

        GameObject gObj = (GameObject) Instantiate(particles, target.transform.position + new Vector3(0.0f, 0.0f, -0.075f), Quaternion.identity);
        Destroy(gObj, duration);
    }

    public static void SpawnSpellParticles(GameObject particles, int duration, Feld target)
    {
        if (particles == null)
            return;

        Quaternion rotation = new Quaternion(0.7f, 0.5f, 0.7f, -0.7f);

        GameObject gObj = (GameObject)Instantiate(particles, target.transform.position + new Vector3(0.0f, 0.0f, -0.075f), Quaternion.identity);
        Destroy(gObj, duration);
    }
}
