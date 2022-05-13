using UnityEngine;
using UnityEngine.VFX;

public class Explosion : MonoBehaviour
{
    public VisualEffect explodeEffect;
    //public readonly ExposedProperty onExplodeProperty = "OnExplode";

    // Start is called before the first frame update
    void Start()
    {
        explodeEffect.SendEvent("OnExplode");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

}
