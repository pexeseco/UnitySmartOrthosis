using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateOBJ_v2 : MonoBehaviour
{
    // Veja que os métodos são estáticos, ou seja, não é necessário instanciar esta classe para utilizá-los.

    public static void UpdateImportedObject(GameObject importedObject){

        // Renomeia o objeto com o nome de seu pai
        importedObject.name = importedObject.transform.parent.name;

        RemoveAllParents(importedObject);
        
    }

    public static void RemoveAllParents(GameObject obj)
    {
        
        // Verifica se o objeto tem um pai
        if (obj.transform.parent != null)
        {
            // Armazena o pai do objeto
            Transform parentTransform = obj.transform.parent;

            // Desassocia o objeto do seu pai
            obj.transform.SetParent(null);

            // Chama recursivamente para remover os pais do pai
            RemoveAllParents(parentTransform.gameObject);

            // Destroi o pai
            Destroy(parentTransform.gameObject);
        }
    }
}
