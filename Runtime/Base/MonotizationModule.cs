using THEBADDEST.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Monotization/MonotizationModule", fileName = "MonotizationModule", order = 1)]
public abstract class MonotizationModule : ScriptableObject
{
    public  virtual async UTask Initialize(){}
}