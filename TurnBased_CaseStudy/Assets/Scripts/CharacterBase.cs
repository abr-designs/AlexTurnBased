using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterBase : MonoBehaviour
{
    [SerializeField, ReadOnly]
    protected STATE currentState;
    
    [Required, SerializeField]
    protected CharacterStats stats;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

}
