using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class RisingText : MonoBehaviour
{
    [SerializeField]
    private float riseSpeed = 1f;
    [SerializeField]
    private float lifeTime = 1f;
    [SerializeField]
    private AnimationCurve _curve = new AnimationCurve();
    
    private Color startColor;


    private TextMeshPro _textMesh;
    
    private new Transform transform;
    // Start is called before the first frame update
    public void Init(string text, Color color, Vector3 position)
    {
        transform = gameObject.transform;
        transform.position = position;
        
        _textMesh = GetComponent<TextMeshPro>();
        _textMesh.text = text;
        startColor = _textMesh.color = color;

        StartCoroutine(FloatCoroutine(lifeTime));
    }

    IEnumerator FloatCoroutine(float time)
    {
        float _t = 0;

        while (_t < time)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            _textMesh.color = Color.Lerp(startColor, Color.clear, _curve.Evaluate((_t += Time.deltaTime) / time));
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
