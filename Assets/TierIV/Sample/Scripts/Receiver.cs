using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TierIV.Event;

namespace TierIV.Sample
{

    public class Receiver : MonoBehaviour
    {
        float rotTime = 0f;
        void Awake()
        {
        }


        void EventReceiver(string text)
        {

        }

        void OnEnable()
        {
            EventNotifier.Instance.SubscribeEvent(this.EventReceiver, "Player");
        }

        void OnDisable()
        {
            EventNotifier.Instance.UnSubscribeEvent(this.EventReceiver);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            var nt = Time.realtimeSinceStartup;
            if (1f < nt - rotTime)
            {
                transform.eulerAngles += new Vector3(0f, 1f, 0f);
            }

        }
    }
}