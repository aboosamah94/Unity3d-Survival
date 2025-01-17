using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialHelper 
{
    public void SwapToSelectionMaterial(GameObject objectToModify, List<Material[]> currentColliderMaterailsList, Material selectionMaterial)
    {
        currentColliderMaterailsList.Clear();
        PrepareRendererToSwapMaterials(objectToModify, currentColliderMaterailsList, selectionMaterial);
        if (objectToModify.transform.childCount > 0)
        {
            foreach (Transform child in objectToModify.transform)
            {
                if(child.gameObject.activeSelf)
                {
                    PrepareRendererToSwapMaterials(child.gameObject, currentColliderMaterailsList, selectionMaterial);
                }
            }
        }
        //else
        //{
        //    PrepareRendererToSwapMaterials(objectToModify, currentColliderMaterailsList, selectionMaterial);
        //}
    }

    public void PrepareRendererToSwapMaterials(GameObject objectToModify, List<Material[]> currentColliderMaterailsList, Material selectionMaterial)
    {
        var renderer = objectToModify.GetComponent<Renderer>();
        currentColliderMaterailsList.Add(renderer.sharedMaterials);
        SwapMaterials(renderer, selectionMaterial);
    }

    public void SwapMaterials(Renderer renderer, Material selectionMaterial)
    {
        Material[] matArray = new Material[renderer.materials.Length];
        for (int i = 0; i < matArray.Length; i++)
        {
            matArray[i] = selectionMaterial;
        }
        renderer.materials = matArray;
    }

    public void SwapToOriginalMaterial(GameObject objectToModify, List<Material[]> currentColliderMaterailsList)
    {
        var renderer = objectToModify.GetComponent<Renderer>();
        renderer.materials = currentColliderMaterailsList[0];
        if (currentColliderMaterailsList.Count > 1)
        {
            for (int i = 0; i < currentColliderMaterailsList.Count; i++)
            {
                if(objectToModify.transform.GetChild(i).gameObject.activeSelf)
                {
                    var childRenderer = objectToModify.transform.GetChild(i).GetComponent<Renderer>();
                    childRenderer.materials = currentColliderMaterailsList[i];
                }
            }
        }
        //else
        //{
        //    var renderer = objectToModify.GetComponent<Renderer>();
        //    renderer.materials = currentColliderMaterailsList[0];
        //}
    }

    public void EnableEmission(GameObject gameObject, Color color)
    {
        var gameObjectrenderer = gameObject.GetComponent<Renderer>();
        for(int i = 0; i < gameObjectrenderer.materials.Length; i++)
        {
            gameObjectrenderer.materials[i].EnableKeyword("_EMISSION");
            gameObjectrenderer.materials[i].SetColor("_EmissionColor", color);
            
        }
    }    
    
    public void DisableEmission(GameObject gameObject)
    {
        var gameObjectrenderer = gameObject.GetComponent<Renderer>();
        for(int i = 0; i < gameObjectrenderer.materials.Length; i++)
        {
            gameObjectrenderer.materials[i].DisableKeyword("_EMISSION");
        }
    }
}
