using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.MonetizationApi
{

    
    public  abstract class MonetizationModule : ScriptableObject
    {

        public virtual async UTask Initialize()
        {
        }

        internal virtual void UpdateModule(){}

    }


}