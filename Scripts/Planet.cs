using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    public double mass;
    public double initVelocity;
    public Vector3 initPos;
    public string tag;


    Planet Init (string tag) {
        this.tag = tag;
    }

    Planet Init (double mass, double velocity, Vector3 pos, string tag){
        init(tag);
        this.mass = mass;
        this.initVelocity = velocity;
        this.initPos = pos;
    }

    void MovePlanetRelativeTo(GameObject target){
        transform.position = target.InverseTransformPoint(transform.position);
    }

    void OnCollisionEnter(Collision collisionTarget, GameObject target) {
        if (collisionTarget.gameObject.tag = "Portal") {
            movePlanetRelativeTo(target);
        } 
    }
}

