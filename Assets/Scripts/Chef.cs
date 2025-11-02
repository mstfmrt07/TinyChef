using System;
using UnityEngine;

namespace TinyChef
{
    public class Chef : MonoBehaviour
    {
        public Transform mainBody;
        public float interactCooldown;
        public LayerMask interactionLayers;
        public InputController inputController;
        public Transform grabPoint;

        private float interactTimer = 0f;
        private IInteractable currentSelection;
        private Ingredient currentIngredient;
        public Ingredient CurrentIngredient => currentIngredient;

        private void Start()
        {
            interactTimer = interactCooldown;
        }

        private void Update()
        {
            Rotate();
            CheckInteract();
        }

        private void Rotate()
        {
            var input = inputController.MovementInput;
            if (input.magnitude == 0)
            {
                return;
            }

            mainBody.eulerAngles = new Vector3(0, Mathf.Atan2(input.x, input.y) * 180 / Mathf.PI, 0);
            if (Physics.Raycast(mainBody.position, mainBody.forward,
                    out RaycastHit hit, 5f,
                    interactionLayers))
            {
                var nextSelection = hit.transform.gameObject.GetComponent<IInteractable>();
                if (currentSelection == nextSelection)
                {
                    return;
                }

                if (currentSelection != null)
                {
                    currentSelection.Deselect();
                }

                currentSelection = nextSelection;
                currentSelection.Select();
            }
        }

        private void CheckInteract()
        {
            if (interactTimer < interactCooldown)
            {
                interactTimer += Time.deltaTime;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (currentSelection == null)
                        return;

                    Interact(currentSelection);
                    interactTimer = 0f;
                }
            }
        }

        public void Interact(IInteractable interactable)
        {
            interactable.Interact();
        }

        public void GrabItem(Ingredient ingredient)
        {
            Debug.Log("Item Grabbed");
            currentIngredient = ingredient;
            currentIngredient.transform.SetParent(grabPoint);
            currentIngredient.transform.localPosition = Vector3.zero;
        }

        public void DropItem()
        {
            Debug.Log("Item Dropped");
            currentIngredient.transform.SetParent(null);
            currentIngredient = null;
        }
    }
}