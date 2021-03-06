﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUser : MonoBehaviour {

    static List<PowerUser> users = new List<PowerUser>();

    protected virtual void Start() {
        users.Add(this);
    }

    public virtual void SetPower(bool powered) {
    }

    public static void OffAll() {
        for (int i = 0; i < users.Count; ++i) {
            users[i].SetPower(false);
        }
    }

}

public enum CType {
    WALL,
    PLAYER,
    USER,
    SOURCE,
    NULL
}


public class Wire : MonoBehaviour {

    public CType leftType = CType.NULL;
    public CType rightType = CType.NULL;

    public Wire leftWire = null;
    public Wire rightWire = null;

    public PowerUser leftPow = null;
    public PowerUser rightPow = null;

    public bool connectedToWall = false;
    public bool powered = false;

    [HideInInspector]
    public float leftMotor;
    [HideInInspector]
    public float rightMotor;

    public AudioClip generalConnect;
    public AudioClip sourceConnect;
    public AudioClip userConnect;

    JointBuilder jb;

    // Use this for initialization
    void Start() {
        wires.Add(this);
        jb = GetComponent<JointBuilder>();
    }

    // Update is called once per frame
    void Update() {

    }

    static List<Wire> wires = new List<Wire>();

    public void Disconnect(bool rightArm) {
        if (rightArm) {
            rightType = CType.NULL;
            rightWire = null;
            rightPow = null;
        } else {
            leftType = CType.NULL;
            leftWire = null;
            leftPow = null;
        }

        UpdateAllWires();
    }

    public void Connect(bool rightArm, Collider2D col) {
        CType type = CType.WALL; // default to wall
        if (col.CompareTag("Player")) {
            if (rightArm) {
                jb.sourceRight.clip = generalConnect;
                jb.sourceRight.Play();
            } else {
                jb.sourceLeft.clip = generalConnect;
                jb.sourceLeft.Play();
            }

            type = CType.PLAYER;
            Wire w = col.transform.parent.GetComponent<Wire>();
            if (w) {
                if (rightArm) {
                    rightWire = w;
                } else {
                    leftWire = w;
                }
            }
        } else if (col.CompareTag("User")) {
            if (rightArm) {
                jb.sourceRight.clip = userConnect;
                jb.sourceRight.Play();
            } else {
                jb.sourceLeft.clip = userConnect;
                jb.sourceLeft.Play();
            }
            PowerUser pu = col.GetComponent<PowerUser>();
            if (pu) {
                if (rightArm) {
                    rightPow = pu;
                } else {
                    leftPow = pu;
                }
            }
            type = CType.USER;
        } else if (col.CompareTag("Source")) {
            if (rightArm) {
                jb.sourceRight.clip = sourceConnect;
                jb.sourceRight.Play();
            } else {
                jb.sourceLeft.clip = sourceConnect;
                jb.sourceLeft.Play();
            }
            type = CType.SOURCE;
        } else {
            if (rightArm) { // wall connect
                jb.sourceRight.clip = generalConnect;
                jb.sourceRight.Play();
            } else {
                jb.sourceLeft.clip = generalConnect;
                jb.sourceLeft.Play();
            }
        }

        if (rightArm) {
            rightType = type;
        } else {
            leftType = type;
        }

        UpdateAllWires();
    }

    void UpdateConnection() {
        // wall is WALL USER or SOURCE (below is demorgans of that)
        connectedToWall = (leftType != CType.PLAYER && leftType != CType.NULL) || (rightType != CType.PLAYER && rightType != CType.NULL);
        powered = leftType == CType.SOURCE || rightType == CType.SOURCE;
    }

    public static void UpdateAllWires() {

        PowerUser.OffAll();

        for (int i = 0; i < wires.Count; ++i) {
            wires[i].UpdateConnection();
        }

        // propogate twice 
        for (int i = 0; i < wires.Count; ++i) {
            wires[i].Propogate();
        }
        for (int i = 0; i < wires.Count; ++i) {
            wires[i].Propogate();
        }

        // check them out??? i dunno i hate my life
        for (int i = 0; i < wires.Count; ++i) {
            if (wires[i].powered) {
                wires[i].CheckPowerStatus();
            }
        }

    }

    public void CheckPowerStatus() {
        if (leftType == CType.USER) {
            leftPow.SetPower(true);
        }
        if (rightType == CType.USER) {
            rightPow.SetPower(true);
        }
    }

    // send out your states
    public void Propogate() {
        if (leftWire) {
            leftWire.connectedToWall |= connectedToWall;
            connectedToWall |= leftWire.connectedToWall;
            leftWire.powered |= powered;
            powered |= leftWire.powered;
        }
        if (rightWire) {
            rightWire.connectedToWall |= connectedToWall;
            connectedToWall |= rightWire.connectedToWall;
            rightWire.powered |= powered;
            powered |= rightWire.powered;
        }
    }

}
