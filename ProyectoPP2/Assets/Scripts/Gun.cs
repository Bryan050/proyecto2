using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnPoint;

    public float shotForce = 15000;
    public float shotRate = 0.05f;

    public float shotRateTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.F))
        {
            if(Time.time > shotRateTime)
            {
                ShootBullet();
            }
        }
    }

    public void ShootBullet()
    {
        GameObject newBullet;
        newBullet = Instantiate(bullet, spawnPoint.position, spawnPoint.rotation);
        newBullet.GetComponent<Rigidbody>().AddForce(spawnPoint.forward*shotForce);
        shotRateTime = Time.time + shotRate;
        Destroy(newBullet, 1);
    }
}
