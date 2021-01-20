using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtremeVR
{

    public class MovePlayer : MonoBehaviour
    {
        private bool _isFrozen = false;
        public bool IsFrozen { get { return _isFrozen; } set { _isFrozen = value;}}
        private CharacterController _controller;
        public float PlayerSpeed = 5.0f;
        //private float h_axis, v_axis;

        // Start is called before the first frame update
        void Start()
        {
            _controller = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
            if(!_isFrozen)
            {
                Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                move = Camera.main.transform.TransformDirection(move);
                move.y = 0.0f;
                if(move != Vector3.zero) _controller.Move(move * Time.deltaTime * PlayerSpeed);
            }
        }
    }
}