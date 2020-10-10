using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;


public class CatAgent : Agent
{
    private int OrderToAction;
  
    public static float love_gauge;
    public  static float condition_gauge;
    public static float fun_gauge;
    public static float loyal_gauge;
    public static int order_count;
    private bool actionFlag = false;
    //recog 이건 손을 인식했을 때 인트값으로 어떤 걸 인식했는지 알리는 것 . 
    private int recognizeFlag;
    private int randomOrder;
    private float recogTime;
    private Animator CatAnimator;
    private bool AnimSwitch;
    public void InitializeGauge()
    {
        condition_gauge = 100f;
        love_gauge = 100f;
        fun_gauge = 100f;
        loyal_gauge = 100f;
        order_count = 30;
        actionFlag = false;
        recognizeFlag = 0;
        CatAnimator = gameObject.GetComponent<Animator>();
        AnimSwitch = false;
        StartCoroutine("minusValues");
        
    }
  
    ////상태 //////////////////////////////////////////////////////////////////
    public void Dead()
    {
            SetReward(-1f);
            EndEpisode();
    }
    //flee is activate when loyal is under zero.
    public void Flee()
    {
            SetReward(-1f);
            EndEpisode();
    }
    //rage is activate when fun is under zero. 
    public void Rage()
    {
            SetReward(-0.5f);
            EndEpisode();
    }
    //////////////////////////////////////////////////////////////////
    
    //명령 실행시 . //////////////////////////////////////////////////////////////////
    public void Sit(){
        love_gauge += 12f;
        StartCoroutine("SitSystem");
    }
    public void Sleep()
    {
        condition_gauge += 15f;
        StartCoroutine("SleepSystem");
    }
    public void Run()
    {
        fun_gauge += 10f;
        StartCoroutine("RunSystem");
    }
    ///////////////////////////////////////////////////////////////////////
    //////order //////////////////////////////////////////////////////////////////
    IEnumerator SleepSystem()
    {
        if(AnimSwitch == false)
        {
            Debug.Log("Sleep");
            CatAnimator.SetBool("isSleeping", true);
            AnimSwitch = true;
        }
        yield return new WaitForSeconds(6);
        if(AnimSwitch == true)
        {
            CatAnimator.SetBool("isSitting", false);
            CatAnimator.SetBool("isRunning", false);
            CatAnimator.SetBool("isSleeping", false);
            AnimSwitch = false;
        }
    }
    IEnumerator RunSystem()
    {
        if(AnimSwitch == false)
        {
            Debug.Log("Run");
            CatAnimator.SetBool("isRunning", true);
            AnimSwitch = true;
        }
        yield return new WaitForSeconds(6);
        if(AnimSwitch == true)
        {
            CatAnimator.SetBool("isSitting", false);
            CatAnimator.SetBool("isRunning", false);
            CatAnimator.SetBool("isSleeping", false);
            AnimSwitch = false;
        }
    }
    IEnumerator SitSystem()
    {
        if(AnimSwitch == false)
        {
            Debug.Log("Sit");
            CatAnimator.SetBool("isSitting", true);
            AnimSwitch = true;
        }
        yield return new WaitForSeconds(6);
        if(AnimSwitch == true)
        {
            CatAnimator.SetBool("isSitting", false);
            CatAnimator.SetBool("isRunning", false);
            CatAnimator.SetBool("isSleeping", false);
            AnimSwitch = false;
        }
    }
    IEnumerator minusValues()
    {
        while(order_count >= 0 && 
        fun_gauge >= 0 && 
        love_gauge >= 0 && 
        condition_gauge >= 0 
        && loyal_gauge >= 0)
        {
            var random1 = Random.Range(1,3);
            var random2 = Random.Range(1,5);
            var random3 = Random.Range(1,7);
            fun_gauge -= random1;
            love_gauge -= random2;
            condition_gauge -= random3;
            yield return new WaitForSeconds(1f);
        }
        
    }
    // IEnumerator RandomClickSystem()
    // {
    //     while(true)
    //     {
    //         randomOrder =  Random.Range(1,4);
    //         if(randomOrder == 1)
    //         {
    //             if(AnimSwitch == false)
    //             {
    //                 Debug.Log("Sleep");
    //                 CatAnimator.SetBool("isSleeping", true);
    //                 AnimSwitch = true;
    //             }
    //         }
    //         else if(randomOrder == 2)
    //         {
    //             if(AnimSwitch == false)
    //             {
    //                 Debug.Log("Run");
    //                 CatAnimator.SetBool("isRunning", true);
    //                 AnimSwitch = true;
    //             }
    //         }
    //         else if(randomOrder == 3)
    //         {
    //             if(AnimSwitch == false)
    //             {
    //                 Debug.Log("Sit");
    //                 CatAnimator.SetBool("isSitting", true);
    //                 AnimSwitch = true;
    //             }
    //         }
    //         yield return new WaitForSeconds(6);
    //         if(AnimSwitch == true)
    //         {
    //             CatAnimator.SetBool("isSitting", false);
    //             CatAnimator.SetBool("isRunning", false);
    //             CatAnimator.SetBool("isSleeping", false);
    //             AnimSwitch = false;
    //         }
    //     } 
    // }
    public void Order() {
        order_count -= 1;
    }
    //////////////////////////////////////////////////////////////////

    ///// ML 설정 . //////////////////////////////////////////////////////////////////
    public override void Initialize()
    {
        InitializeGauge();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(condition_gauge);
        sensor.AddObservation(fun_gauge);
        sensor.AddObservation(love_gauge);
        sensor.AddObservation(loyal_gauge);

        sensor.AddObservation(AnimSwitch);

        sensor.AddObservation(order_count);
        sensor.AddObservation(randomOrder);
    }
    public void ActionCat(ActionSegment<int> action)
    {
        //에피소드 끝내는 것 . 
        if(order_count <= 0)
        {
            EndEpisode();
        }
        if(love_gauge <= 0 || loyal_gauge <= 0)
        {
            Flee();
        }
        if(condition_gauge <= 0)
        {
            Dead();
        }
        if(fun_gauge <= 0)
        {
            Rage();
        }
        //
        Order();
        //
        Debug.Log("awdf");
        
        if(AnimSwitch == false && order_count > 0 )
        {
            randomOrder = Random.Range(1,5);
            //일부러 랜덤 값을 주어서 , 학습에 제약을 주어서 더욱 학습 성능 높히기 . 행동 제약 
            //값을 받아도 다른걸 실행시키지 않게 됨 . 
            //나중에는 내가 여기에 randomOrder 값을 조정해서 명령 내리도록 하면 될 듯 . 
            OrderToAction = (int)action[0];

            switch (OrderToAction)
            {
                case 1:
                    if(randomOrder == 1)
                    {
                        Sleep();
                        SetReward(0.8f);
                        loyal_gauge += 10f;
                        actionFlag = false;
                    }
                    break;
                case 2:
                    if(randomOrder == 2)
                    {
                        Sit();
                        SetReward(0.5f);
                        loyal_gauge += 10f;
                        actionFlag = false;
                    }
                    break;
                //졸릴때 놀면 더 피곤해짐 . 
                case 3:
                    if(randomOrder ==3 )
                    {
                        Run();
                        SetReward(0.3f);
                        loyal_gauge += 10f;
                        actionFlag = false;
                    }
                    break;
                default:
                    SetReward(-1f);
                    loyal_gauge -= 4f;
                    actionFlag = false;
                    break;
            }
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionCat(actions.DiscreteActions);
    }

    public override void OnEpisodeBegin()
    {
        InitializeGauge();
    }
}
