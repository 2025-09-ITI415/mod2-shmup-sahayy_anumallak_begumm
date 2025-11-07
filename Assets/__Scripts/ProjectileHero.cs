using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileHero : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Dynamic")]
    public Rigidbody rigid;
    [SerializeField]
    private eWeaponType _type;

    // ---- Phaser support (added) ----
    [HideInInspector] public bool isPhaser = false;   // toggled by Weapon.cs when using phaser
    private float birthTime;                          // for time-based sine
    private float sineAmplitude = 0.6f;               // left-right wave size (tune in Inspector if you want)
    private float sineFrequency = 6f;                 // oscillations per second (tune if needed)
    // --------------------------------

    // This public property masks the private field _type
    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
        birthTime = Time.time;                      // (added) start clock for sine
    }

    void Update()
    {
        // (added) Apply sine-wave drift if this is a Phaser projectile
        if (isPhaser)
        {
            Vector3 pos = transform.position;
            float age = Time.time - birthTime;
            // multiply by Time.deltaTime*10 to keep motion smooth and speed-agnostic
            pos.x += Mathf.Sin(age * sineFrequency) * sineAmplitude * Time.deltaTime * 10f;
            transform.position = pos;
        }

        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the _type private field and colors this projectile to match the WeaponDefinition.
    /// </summary>
    /// <param name="eType">The eWeaponType to use.</param>
    public void SetType(eWeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        rend.material.color = def.projectileColor;
    }

    /// <summary>
    /// Allows Weapon to easily set the velocity of this ProjectileHero
    /// </summary>
    public Vector3 vel
    {
        get { return rigid.linearVelocity; }
        set { rigid.linearVelocity = value; }
    }
}
