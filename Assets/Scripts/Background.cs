using UnityEngine;
using System.Collections;

public class Background : MonoBehaviour
{
    public float TextureScale = 20.0f;

    // Use this for initialization
    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.mainTextureScale = new Vector2(TextureScale, TextureScale);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape"))
            Application.Quit();

    }
}
