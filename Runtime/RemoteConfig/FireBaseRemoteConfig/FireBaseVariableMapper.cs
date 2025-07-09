using System;
using System.Globalization;
using Firebase.RemoteConfig;
using UnityEngine;
using THEBADDEST.PrimitiveTypes;
using String = THEBADDEST.PrimitiveTypes.String;


namespace THEBADDEST.RemoteConfigSystem
{


	[CreateAssetMenu(menuName = "THEBADDEST/RemoteConfigSystem/FireBaseVariableMapper", fileName = "FireBaseVariableMapper", order = 0)]
	public class FireBaseVariableMapper : RemoteVariablesMapper
	{
		[SerializeField] private bool enableLogging = false;
		protected override void ConvertPrimitiveToSoValue(RemoteVariable remoteVariable, object targetObject)
		{
			if (targetObject is ConfigValue configValue)
			{
				UpdateValues(remoteVariable, configValue);
			}
		}

		void UpdateValues(RemoteVariable remoteVariable, ConfigValue configValue)
		{
			
			string remoteVariableValue="";
			switch (remoteVariable.type)
			{
				case RemoteVariableType.Boolean:
					remoteVariable.booleanValue = configValue.BooleanValue;
					remoteVariableValue = configValue.BooleanValue.ToString();
					break;

				case RemoteVariableType.Long:
					remoteVariable.longValue = (int)configValue.LongValue;
					remoteVariableValue = configValue.LongValue.ToString();
					break;

				case RemoteVariableType.Double:
					remoteVariable.doubleValue = ((float)configValue.DoubleValue);
					remoteVariableValue = configValue.DoubleValue.ToString(CultureInfo.InvariantCulture);
					break;

				case RemoteVariableType.StringValue:
					remoteVariable.stringValue=configValue.StringValue;
					remoteVariableValue = configValue.StringValue;
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
			if (enableLogging)
			{
				Debug.Log($"<color=green>[Remote Config Var]</color> :  {remoteVariable.type} : {remoteVariableValue}");
			}
			
			
		}

		

		protected override object ConvertSoValueToPrimitive(RemoteVariable remoteVariable)
		{
			switch (remoteVariable.type)
			{
				case RemoteVariableType.Boolean:
					return remoteVariable.booleanValue;

				case RemoteVariableType.Long:
					return remoteVariable.longValue;

				case RemoteVariableType.Double:
					return remoteVariable.doubleValue;

				case RemoteVariableType.StringValue:
					return remoteVariable.stringValue;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

	}


}