using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


    class PipeDestroyerScript : MonoBehaviour
    {
        // Called when another collider enters this trigger collider
        void OnTriggerEnter2D(Collider2D col)
        {
            // Check if the object entering the trigger is a pipe or the pipe gap
            if (col.tag == "Pipe" || col.tag == "Pipeblank")
                Destroy(col.gameObject.transform.parent.gameObject); // Destroy the entire pipe group (parent object)

    }
}

