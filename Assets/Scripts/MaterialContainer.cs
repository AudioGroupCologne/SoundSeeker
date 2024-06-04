using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialContainer : MonoBehaviour
{
    public List<Material> skyboxes;
    public List<Material> floorMaterials;
    public List<Material> targetColors;
    public GameObject floor;
    public GameObject wall1;
    public GameObject wall2;
    public GameObject wall3;
    public GameObject wall4;
    public GameObject singleTarget;

    // Start is called before the first frame update
    void Start()
    {
        int idx = Random.Range(0, skyboxes.Count - 1);
        RenderSettings.skybox = skyboxes[idx];
        idx = Random.Range(0, floorMaterials.Count - 1);
        floor.GetComponent<Renderer>().material = floorMaterials[idx];
        floor.GetComponent<Renderer>().material.mainTextureScale = new Vector2(10, 10);
        singleTarget.GetComponent<Renderer>().material = targetColors[0];

    }

    public void SetTargetColor(float distance)
    {
        if(distance > 3)
        {
            TargetBad();
        } else if (distance < 3 && distance >= 1)
        {
            TargetOk();
        } else
        {
            TargetPerfect();
        }
    }

    private void TargetPerfect()
    {
        singleTarget.GetComponent<Renderer>().material = targetColors[0];
    }

    private void TargetOk()
    {
        singleTarget.GetComponent<Renderer>().material = targetColors[1];

    }

    private void TargetBad()
    {
        singleTarget.GetComponent<Renderer>().material = targetColors[2];

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
