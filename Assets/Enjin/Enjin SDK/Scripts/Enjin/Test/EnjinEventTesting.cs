//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using EnjinSDK;

//public class EnjinEventTesting : MonoBehaviour {

//    private Action<RequestEvent> listener1;
//    private Action<RequestEvent> listener2;
//    private Action<RequestEvent> listener3;

//    private EnjinEventManager enjinEventManager;

//    void Awake()
//    {
//        listener1 = new Action<RequestEvent>(funcOne);

//        enjinEventManager = EnjinSDK.EnjinEventManager.instance;

//        // test in game...
//        StartCoroutine(invokeTest());
//    }

//    IEnumerator invokeTest()
//    {
//        WaitForSeconds wt = new WaitForSeconds(0.5f);

//        RequestEvent request = new RequestEvent();
        

//        while(true)
//        {
//            yield return wt;

//            request.model = "model";
//            request.event_type = "event_type";
//            request.contract = "contract";
//            request.data = new RequestEventData();
//            request.request_id = 101;

//            enjinEventManager.TriggerEvent("test01", request);
//            yield return wt;

//            request.model = "model";
//            request.event_type = "event_type";
//            request.contract = "contract";
//            request.data = new RequestEventData();
//            request.request_id = 102;

//            enjinEventManager.TriggerEvent("test02", request);
//            yield return wt;

//            request.model = "model";
//            request.event_type = "event_type";
//            request.contract = "contract";
//            request.data = new RequestEventData();
//            request.request_id = 103;

//            enjinEventManager.TriggerEvent("test03", request);
//        }
//    }

//    void OnEnable()
//    {
//        EnjinEventManager.StartListening("test01", funcOne);
//        EnjinEventManager.StartListening("test02", funcTwo);
//        EnjinEventManager.StartListening("test03", funcThree);
//    }

//    void OnDisable()
//    {
//        EnjinEventManager.StopListening("test01", funcOne);
//    }

//    void funcOne(RequestEvent request)
//    {
//        Debug.Log("001 - Called our test function for " + request.request_id + ": " + request.model);
//    }

//    void funcTwo(RequestEvent request)
//    {
//        Debug.Log("002 - Called our test function for " + request.request_id + ": " + request.model);
//    }

//    void funcThree(RequestEvent request)
//    {
//        Debug.Log("003 - Called our test function for " + request.request_id + ": " + request.model);
//    }
//}
