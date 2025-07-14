using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorkerTask
{
    None,Building,Chopping
}


public class WorkerUnit : HumanoidUnit
{
    public WorkerTask currentTask = WorkerTask.None;
   
    protected override void Update()
    {
        base.Update();
    }

    protected override void UpdateBehaviour()
    {
        if(Time.time - CheckTimer >= CheckFrequency )
        {
            CheckTimer = Time.time;

            if (HasRegisteredTarget)
                MoveToDestination(Target.transform.position);

            if(IsBuildingDetected())
            {
                if(currentTask == WorkerTask.Building)
                {
                    anim.SetBool("Build",true);
                    StartBuildingProcess(Target as StructureUnit);
                }
                else if(currentTask == WorkerTask.Chopping)
                {
                    anim.SetBool("Chop",true);
                }
                
            }
            else
            {
                anim.SetBool("Build",false);
                anim.SetBool("Chop",false);
            }

        }
    }

    public void StartBuildingProcess(StructureUnit _structure)
    {
        _structure.AssignWorker(this);
    }

    public void ChopTree()
    {
        if(HasRegisteredTarget)
        {
            (Target as TreeUnit).Shake();
        }
    }



}
