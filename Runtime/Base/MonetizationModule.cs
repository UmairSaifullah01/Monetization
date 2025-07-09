using THEBADDEST.Tasks;
using UnityEngine;


namespace THEBADDEST.Monetization
{


    [CreateAssetMenu(menuName = "Monotization/MonotizationModule", fileName = "MonotizationModule", order = 1)]
    public abstract class MonetizationModule : ScriptableObject
    {

        public virtual async UTask Initialize()
        {
        }

    }

    // SendLog static class for colored tagged logging


}