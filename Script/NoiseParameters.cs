using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NoiseParameters", menuName = "NoiseParameters")]
public class NoiseParameters : ScriptableObject {

    public float lacunarity;
    public float persistance;
    public float scale;
    public int octaves;
    public int seed;

    public bool blur;
    public int radius;
    public int iternation;

    public bool rig;
    public int rigExponent;
    public int rigOctaves;
}
