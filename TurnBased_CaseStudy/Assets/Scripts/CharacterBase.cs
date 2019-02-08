using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Character_BaseStats", menuName = "Character/Stats", order = 1)]
public class CharacterBase : MonoBehaviour
{
    public Transform Transform => transform;

    protected STATE currentState;

    [SerializeField]
    protected TYPE attackType;
    
    [Required, SerializeField]
    protected CharacterStats stats;

    private new Transform transform;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        transform = gameObject.transform;
    }

}
