using System;
using UnityEngine;

namespace TinyChef
{
    public class InputController : MonoBehaviour
    {
        public Vector2 MovementInput => new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        private void Update()
        {
        }
    }
}